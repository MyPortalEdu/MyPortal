using System.Data;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

/// <summary>
/// The Medical area of the student profile. Writes the medical-needs flag on the shared Person — via
/// load-modify-save so the other person fields are preserved — and reconciles three child collections
/// (medical conditions, dietary requirements, disabilities) in one transaction. Authorization is
/// enforced at the controller (Student.{View|Edit}StudentMedical).
/// </summary>
public class StudentMedicalService(
    IAuthorizationService authorizationService,
    ILogger<StudentMedicalService> logger,
    IStudentRepository studentRepository,
    IPersonRepository personRepository,
    IPersonConditionRepository personConditionRepository,
    IPersonDietaryRequirementRepository personDietaryRequirementRepository,
    IPersonDisabilityRepository personDisabilityRepository,
    IMedicalConditionRepository medicalConditionRepository,
    IDietaryRequirementRepository dietaryRequirementRepository,
    IDisabilityRepository disabilityRepository,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IStudentMedicalService
{
    public async Task<StudentMedicalDetailsResponse> GetMedicalDetailsAsync(Guid studentId,
        CancellationToken cancellationToken)
    {
        var (_, person) = await LoadAsync(studentId, cancellationToken);

        var conditions = await personConditionRepository.GetByPersonIdAsync(person.Id, cancellationToken);
        var dietaryRequirements =
            await personDietaryRequirementRepository.GetByPersonIdAsync(person.Id, cancellationToken);
        var disabilities = await personDisabilityRepository.GetByPersonIdAsync(person.Id, cancellationToken);

        var medicalConditionLookup = await medicalConditionRepository.GetListAsync(cancellationToken: cancellationToken);
        var dietaryRequirementLookup =
            await dietaryRequirementRepository.GetListAsync(cancellationToken: cancellationToken);
        var disabilityLookup = await disabilityRepository.GetListAsync(cancellationToken: cancellationToken);

        return new StudentMedicalDetailsResponse
        {
            HasMedicalNeeds = person.HasMedicalNeeds,
            Conditions = conditions.Select(c => new PersonConditionItem
            {
                MedicalConditionId = c.MedicalConditionId,
                RequiresMedication = c.RequiresMedication,
                Medication = c.Medication,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                InfoReceivedDate = c.InfoReceivedDate,
                Notes = c.Notes
            }).ToList(),
            DietaryRequirementIds = dietaryRequirements.Select(d => d.DietaryRequirementId).ToList(),
            DisabilityIds = disabilities.Select(d => d.DisabilityId).ToList(),
            MedicalConditions = medicalConditionLookup.ToAlphabeticalLookup(),
            DietaryRequirements = dietaryRequirementLookup.ToAlphabeticalLookup(),
            Disabilities = disabilityLookup.ToOrderedLookup()
        };
    }

    public async Task UpdateMedicalDetailsAsync(Guid studentId, StudentMedicalDetailsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(model);

        var (_, person) = await LoadAsync(studentId, cancellationToken);

        person.HasMedicalNeeds = model.HasMedicalNeeds;

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await personRepository.UpdateAsync(person, cancellationToken, uow.Transaction);
            await ReconcileConditionsAsync(person.Id, model.Conditions, uow.Transaction, cancellationToken);
            await ReconcileDietaryRequirementsAsync(person.Id, model.DietaryRequirementIds, uow.Transaction,
                cancellationToken);
            await ReconcileDisabilitiesAsync(person.Id, model.DisabilityIds, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    private async Task ReconcileConditionsAsync(Guid personId, List<PersonConditionItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        // Rich reconcile keyed by MedicalConditionId — last write wins on duplicate keys.
        var wanted = incoming
            .GroupBy(c => c.MedicalConditionId)
            .ToDictionary(g => g.Key, g => g.Last());

        var existing = await personConditionRepository.GetByPersonIdAsync(personId, cancellationToken, transaction);
        var existingIds = existing.Select(e => e.MedicalConditionId).ToHashSet();

        foreach (var row in existing)
        {
            if (wanted.TryGetValue(row.MedicalConditionId, out var item))
            {
                // Present in the payload — update the detail on the existing row.
                Apply(row, item);
                await personConditionRepository.UpdateAsync(row, cancellationToken, transaction);
            }
            else
            {
                // Dropped by the payload — hard-delete (the join row carries no soft-delete column).
                await personConditionRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
            }
        }

        foreach (var (medicalConditionId, item) in wanted)
        {
            if (!existingIds.Contains(medicalConditionId))
            {
                var row = new PersonCondition
                {
                    Id = SqlConvention.SequentialGuid(),
                    PersonId = personId,
                    MedicalConditionId = medicalConditionId
                };
                Apply(row, item);
                await personConditionRepository.InsertAsync(row, cancellationToken, transaction);
            }
        }
    }

    // Copy the editable condition detail onto the row. Medication is meaningful only when the
    // condition requires it — otherwise it's dropped so a cleared flag can't leave stale text.
    private static void Apply(PersonCondition row, PersonConditionItem item)
    {
        row.RequiresMedication = item.RequiresMedication;
        row.Medication = item.RequiresMedication ? item.Medication?.Trim() : null;
        row.StartDate = item.StartDate;
        row.EndDate = item.EndDate;
        row.InfoReceivedDate = item.InfoReceivedDate;
        row.Notes = string.IsNullOrWhiteSpace(item.Notes) ? null : item.Notes.Trim();
    }

    private async Task ReconcileDietaryRequirementsAsync(Guid personId, List<Guid> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var wanted = incoming.Distinct().ToHashSet();

        var existing =
            await personDietaryRequirementRepository.GetByPersonIdAsync(personId, cancellationToken, transaction);
        var existingIds = existing.Select(e => e.DietaryRequirementId).ToHashSet();

        foreach (var row in existing)
        {
            if (!wanted.Contains(row.DietaryRequirementId))
            {
                await personDietaryRequirementRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
            }
        }

        foreach (var dietaryRequirementId in wanted)
        {
            if (!existingIds.Contains(dietaryRequirementId))
            {
                await personDietaryRequirementRepository.InsertAsync(new PersonDietaryRequirement
                {
                    Id = SqlConvention.SequentialGuid(),
                    PersonId = personId,
                    DietaryRequirementId = dietaryRequirementId
                }, cancellationToken, transaction);
            }
        }
    }

    private async Task ReconcileDisabilitiesAsync(Guid personId, List<Guid> incoming, IDbTransaction? transaction,
        CancellationToken cancellationToken)
    {
        var wanted = incoming.Distinct().ToHashSet();

        var existing = await personDisabilityRepository.GetByPersonIdAsync(personId, cancellationToken, transaction);
        var existingIds = existing.Select(e => e.DisabilityId).ToHashSet();

        foreach (var row in existing)
        {
            if (!wanted.Contains(row.DisabilityId))
            {
                await personDisabilityRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
            }
        }

        foreach (var disabilityId in wanted)
        {
            if (!existingIds.Contains(disabilityId))
            {
                await personDisabilityRepository.InsertAsync(new PersonDisability
                {
                    Id = SqlConvention.SequentialGuid(),
                    PersonId = personId,
                    DisabilityId = disabilityId
                }, cancellationToken, transaction);
            }
        }
    }

    private async Task<(Core.Entities.Student Student, Core.Entities.Person Person)> LoadAsync(Guid studentId,
        CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdAsync(studentId, cancellationToken);

        if (student == null)
        {
            throw new NotFoundException("Student not found.");
        }

        var person = await personRepository.GetByIdAsync(student.PersonId, cancellationToken);

        if (person == null)
        {
            throw new NotFoundException("Student not found.");
        }

        return (student, person);
    }
}
