using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

public class StudentFamilyService(
    IAuthorizationService authorizationService,
    ILogger<StudentFamilyService> logger,
    IStudentRepository studentRepository,
    IContactRepository contactRepository,
    IStudentContactRelationshipRepository relationshipRepository,
    IRelationshipTypeRepository relationshipTypeRepository,
    IValidationService validationService)
    : BaseService(authorizationService, logger), IStudentFamilyService
{
    public async Task<StudentFamilyResponse> GetFamilyAsync(Guid studentId, CancellationToken cancellationToken)
    {
        await RequireStudentAsync(studentId, cancellationToken);

        var contacts = await relationshipRepository.GetByStudentIdAsync(studentId, cancellationToken);
        var siblings = await relationshipRepository.GetSiblingsByStudentIdAsync(studentId, cancellationToken);
        var relationshipTypes = await relationshipTypeRepository.GetListAsync(cancellationToken: cancellationToken);

        return new StudentFamilyResponse
        {
            Contacts = contacts.ToList(),
            Siblings = siblings.ToList(),
            RelationshipTypes = relationshipTypes.ToOrderedLookup()
        };
    }

    public async Task<Guid> AddRelationshipAsync(Guid studentId, StudentContactRelationshipUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(model);
        await RequireStudentAsync(studentId, cancellationToken);
        await RequireContactAsync(model.ContactId, cancellationToken);

        var existing = await relationshipRepository.GetByStudentIdAsync(studentId, cancellationToken);

        if (existing.Any(r => r.ContactId == model.ContactId))
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(model.ContactId), "This contact is already linked to the student.")]);
        }

        var relationship = new StudentContactRelationship
        {
            Id = SqlConvention.SequentialGuid(),
            StudentId = studentId,
            ContactId = model.ContactId,
            RelationshipTypeId = model.RelationshipTypeId,
            HasCorrespondence = model.HasCorrespondence,
            HasParentalResponsibility = model.HasParentalResponsibility,
            HasPupilReport = model.HasPupilReport,
            HasCourtOrder = model.HasCourtOrder,
            ContactOrder = model.ContactOrder
        };

        await relationshipRepository.InsertAsync(relationship, cancellationToken);

        return relationship.Id;
    }

    public async Task UpdateRelationshipAsync(Guid studentId, Guid relationshipId,
        StudentContactRelationshipUpsertRequest model, CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(model);

        var relationship = await GetOwnedRelationshipAsync(studentId, relationshipId, cancellationToken);

        await RequireContactAsync(model.ContactId, cancellationToken);

        // Changing the contact must not collide with another of the student's existing links.
        if (relationship.ContactId != model.ContactId)
        {
            var existing = await relationshipRepository.GetByStudentIdAsync(studentId, cancellationToken);

            if (existing.Any(r => r.ContactId == model.ContactId && r.Id != relationshipId))
            {
                throw new ValidationException(
                    [new ValidationFailure(nameof(model.ContactId), "This contact is already linked to the student.")]);
            }
        }

        relationship.ContactId = model.ContactId;
        relationship.RelationshipTypeId = model.RelationshipTypeId;
        relationship.HasCorrespondence = model.HasCorrespondence;
        relationship.HasParentalResponsibility = model.HasParentalResponsibility;
        relationship.HasPupilReport = model.HasPupilReport;
        relationship.HasCourtOrder = model.HasCourtOrder;
        relationship.ContactOrder = model.ContactOrder;

        await relationshipRepository.UpdateAsync(relationship, cancellationToken);
    }

    public async Task RemoveRelationshipAsync(Guid studentId, Guid relationshipId,
        CancellationToken cancellationToken)
    {
        await GetOwnedRelationshipAsync(studentId, relationshipId, cancellationToken);

        // The join has no soft-delete column — unlinking hard-deletes the relationship row.
        await relationshipRepository.DeleteAsync(relationshipId, cancellationToken, softDelete: false);
    }

    private async Task RequireStudentAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdAsync(studentId, cancellationToken);

        if (student == null)
        {
            throw new NotFoundException("Student not found.");
        }
    }

    private async Task RequireContactAsync(Guid contactId, CancellationToken cancellationToken)
    {
        var contact = await contactRepository.GetByIdAsync(contactId, cancellationToken);

        if (contact == null)
        {
            throw new NotFoundException("Contact not found.");
        }
    }

    private async Task<StudentContactRelationship> GetOwnedRelationshipAsync(Guid studentId, Guid relationshipId,
        CancellationToken cancellationToken)
    {
        var relationship = await relationshipRepository.GetByIdAsync(relationshipId, cancellationToken);

        // A mismatched student id is treated as not-found so a link can't be edited via the wrong student.
        if (relationship == null || relationship.StudentId != studentId)
        {
            throw new NotFoundException("Contact relationship not found.");
        }

        return relationship;
    }
}
