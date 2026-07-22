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
/// The SEN area of the student profile. Maintains the dated SEN status history (with a denormalised
/// current-status cache on the Student) and reconciles three child collections — needs, provisions and
/// statutory statements — each in one transaction. Authorization is enforced at the controller
/// (Student.{View|Edit}StudentSen). Reviews are a later increment and are not handled here.
/// </summary>
public class StudentSenService(
    IAuthorizationService authorizationService,
    ILogger<StudentSenService> logger,
    IStudentRepository studentRepository,
    IStudentSenNeedRepository senNeedRepository,
    ISenStatusHistoryRepository senStatusHistoryRepository,
    ISenProvisionRepository senProvisionRepository,
    ISenStatementRepository senStatementRepository,
    ISenStatusRepository senStatusRepository,
    ISenTypeRepository senTypeRepository,
    ISenProvisionTypeRepository senProvisionTypeRepository,
    ISenStatutoryAssessmentAgreedRepository senStatutoryAssessmentAgreedRepository,
    ISenStatutoryAssessmentOutcomeRepository senStatutoryAssessmentOutcomeRepository,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IStudentSenService
{
    public async Task<StudentSenDetailsResponse> GetSenDetailsAsync(Guid studentId,
        CancellationToken cancellationToken)
    {
        var student = await LoadAsync(studentId, cancellationToken);

        var statusHistory = await senStatusHistoryRepository.GetByStudentIdAsync(studentId, cancellationToken);
        var needs = await senNeedRepository.GetByStudentIdAsync(studentId, cancellationToken);
        var provisions = await senProvisionRepository.GetByStudentIdAsync(studentId, cancellationToken);
        var statements = await senStatementRepository.GetByStudentIdAsync(studentId, cancellationToken);

        var senStatuses = await senStatusRepository.GetListAsync(cancellationToken: cancellationToken);
        var senTypes = await senTypeRepository.GetListAsync(cancellationToken: cancellationToken);
        var senProvisionTypes = await senProvisionTypeRepository.GetListAsync(cancellationToken: cancellationToken);
        var assessmentAgreed =
            await senStatutoryAssessmentAgreedRepository.GetListAsync(cancellationToken: cancellationToken);
        var assessmentOutcome =
            await senStatutoryAssessmentOutcomeRepository.GetListAsync(cancellationToken: cancellationToken);

        return new StudentSenDetailsResponse
        {
            StudentId = studentId,
            CurrentSenStatusId = student.SenStatusId,
            SenStartDate = student.SenStartDate,
            SenUnitMember = student.SenUnitMember,
            ResourcedProvisionMember = student.ResourcedProvisionMember,
            StatusHistory = statusHistory
                .OrderByDescending(h => h.EndDate == null)
                .ThenByDescending(h => h.StartDate)
                .Select(MapStatusHistory)
                .ToList(),
            Needs = needs
                .OrderBy(n => n.Rank)
                .Select(MapNeed)
                .ToList(),
            Provisions = provisions
                .OrderByDescending(p => p.StartDate)
                .Select(MapProvision)
                .ToList(),
            Statements = statements
                .OrderByDescending(s => s.AssessmentRequestDate)
                .Select(MapStatement)
                .ToList(),
            SenStatuses = senStatuses.ToAlphabeticalLookup(),
            SenTypes = senTypes.ToOrderedLookup(),
            SenProvisionTypes = senProvisionTypes.ToAlphabeticalLookup(),
            StatutoryAssessmentAgreedOptions = assessmentAgreed.ToAlphabeticalLookup(),
            StatutoryAssessmentOutcomeOptions = assessmentOutcome.ToAlphabeticalLookup()
        };
    }

    public async Task SetSenStatusAsync(Guid studentId, SetSenStatusRequest model,
        CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(model);

        var student = await LoadAsync(studentId, cancellationToken);

        // Unchanged status — leave the history untouched so we don't duplicate the open row.
        if (student.SenStatusId == model.SenStatusId)
        {
            return;
        }

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var history =
                await senStatusHistoryRepository.GetByStudentIdAsync(studentId, cancellationToken, uow.Transaction);

            // Close any currently-open row(s); the new status starts where the old one ends.
            foreach (var open in history.Where(h => h.EndDate == null))
            {
                open.EndDate = model.StartDate;
                await senStatusHistoryRepository.UpdateAsync(open, cancellationToken, uow.Transaction);
            }

            await senStatusHistoryRepository.InsertAsync(new SenStatusHistory
            {
                Id = SqlConvention.SequentialGuid(),
                StudentId = studentId,
                SenStatusId = model.SenStatusId,
                StartDate = model.StartDate,
                EndDate = null
            }, cancellationToken, uow.Transaction);

            student.SenStatusId = model.SenStatusId;
            student.SenStartDate = model.StartDate;
            await studentRepository.UpdateAsync(student, cancellationToken, uow.Transaction);
        }, cancellationToken);
    }

    public async Task UndoLatestSenStatusAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var student = await LoadAsync(studentId, cancellationToken);

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var history =
                (await senStatusHistoryRepository.GetByStudentIdAsync(studentId, cancellationToken, uow.Transaction))
                .ToList();

            // The current status is the open row; if nothing is recorded there's nothing to undo.
            var current = history
                .Where(h => h.EndDate == null)
                .OrderByDescending(h => h.StartDate)
                .FirstOrDefault();

            if (current == null)
            {
                return;
            }

            await senStatusHistoryRepository.DeleteAsync(current.Id, cancellationToken, false, uow.Transaction);

            // Re-open the previous entry (if any) and restore the current-status cache from it.
            var previous = history
                .Where(h => h.Id != current.Id)
                .OrderByDescending(h => h.StartDate)
                .FirstOrDefault();

            if (previous != null)
            {
                previous.EndDate = null;
                await senStatusHistoryRepository.UpdateAsync(previous, cancellationToken, uow.Transaction);
                student.SenStatusId = previous.SenStatusId;
                student.SenStartDate = previous.StartDate;
            }
            else
            {
                student.SenStatusId = null;
                student.SenStartDate = null;
            }

            await studentRepository.UpdateAsync(student, cancellationToken, uow.Transaction);
        }, cancellationToken);
    }

    public async Task UpdateSenNeedsAsync(Guid studentId, IEnumerable<SenNeedUpsertRequest> model,
        CancellationToken cancellationToken)
    {
        var incoming = model.ToList();

        foreach (var need in incoming)
        {
            await validationService.ValidateAsync(need);
        }

        await LoadAsync(studentId, cancellationToken);

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await ReconcileNeedsAsync(studentId, incoming, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    public async Task UpdateSenProvisionsAsync(Guid studentId, IEnumerable<SenProvisionUpsertRequest> model,
        CancellationToken cancellationToken)
    {
        var incoming = model.ToList();

        foreach (var provision in incoming)
        {
            await validationService.ValidateAsync(provision);
        }

        await LoadAsync(studentId, cancellationToken);

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await ReconcileProvisionsAsync(studentId, incoming, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    public async Task UpdateSenStatementsAsync(Guid studentId, IEnumerable<SenStatementUpsertRequest> model,
        CancellationToken cancellationToken)
    {
        var incoming = model.ToList();

        foreach (var statement in incoming)
        {
            await validationService.ValidateAsync(statement);
        }

        await LoadAsync(studentId, cancellationToken);

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await ReconcileStatementsAsync(studentId, incoming, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    private async Task ReconcileNeedsAsync(Guid studentId, List<SenNeedUpsertRequest> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing = (await senNeedRepository.GetByStudentIdAsync(studentId, cancellationToken, transaction)).ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await senNeedRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyNeed(entity, item);
                await senNeedRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new StudentSenNeed { Id = SqlConvention.SequentialGuid(), StudentId = studentId };
                ApplyNeed(created, item);
                await senNeedRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private async Task ReconcileProvisionsAsync(Guid studentId, List<SenProvisionUpsertRequest> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await senProvisionRepository.GetByStudentIdAsync(studentId, cancellationToken, transaction)).ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await senProvisionRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyProvision(entity, item);
                await senProvisionRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new SenProvision { Id = SqlConvention.SequentialGuid(), StudentId = studentId };
                ApplyProvision(created, item);
                await senProvisionRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private async Task ReconcileStatementsAsync(Guid studentId, List<SenStatementUpsertRequest> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            (await senStatementRepository.GetByStudentIdAsync(studentId, cancellationToken, transaction)).ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await senStatementRepository.DeleteAsync(row.Id, cancellationToken, false, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                ApplyStatement(entity, item);
                await senStatementRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                var created = new SenStatement { Id = SqlConvention.SequentialGuid(), StudentId = studentId };
                ApplyStatement(created, item);
                await senStatementRepository.InsertAsync(created, cancellationToken, transaction);
            }
        }
    }

    private static void ApplyNeed(StudentSenNeed row, SenNeedUpsertRequest item)
    {
        row.SenTypeId = item.SenTypeId;
        row.Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim();
        row.StartDate = item.StartDate;
        row.EndDate = item.EndDate;
        row.Rank = item.Rank;
    }

    private static void ApplyProvision(SenProvision row, SenProvisionUpsertRequest item)
    {
        row.SenProvisionTypeId = item.SenProvisionTypeId;
        row.StartDate = item.StartDate;
        row.EndDate = item.EndDate;
        row.Frequency = string.IsNullOrWhiteSpace(item.Frequency) ? null : item.Frequency.Trim();
        row.Cost = item.Cost;
        row.Note = item.Note.Trim();
    }

    private static void ApplyStatement(SenStatement row, SenStatementUpsertRequest item)
    {
        row.IsEhcp = item.IsEhcp;
        row.AssessmentRequestDate = item.AssessmentRequestDate;
        row.ParentConsultDate = item.ParentConsultDate;
        row.FinalisedDate = item.FinalisedDate;
        row.CeasedDate = item.CeasedDate;
        row.StatutoryAssessmentAgreedId = item.StatutoryAssessmentAgreedId;
        row.StatutoryAssessmentOutcomeId = item.StatutoryAssessmentOutcomeId;
        row.SubjectToTribunal = item.SubjectToTribunal;
        row.UndergoingMediation = item.UndergoingMediation;
        row.AppealNotes = string.IsNullOrWhiteSpace(item.AppealNotes) ? null : item.AppealNotes.Trim();
        row.TemporaryDisapplicationSubjects = string.IsNullOrWhiteSpace(item.TemporaryDisapplicationSubjects)
            ? null
            : item.TemporaryDisapplicationSubjects.Trim();
        row.PermanentDisapplicationSubjects = string.IsNullOrWhiteSpace(item.PermanentDisapplicationSubjects)
            ? null
            : item.PermanentDisapplicationSubjects.Trim();
        row.LocalAuthorityId = item.LocalAuthorityId;
        row.Comments = string.IsNullOrWhiteSpace(item.Comments) ? null : item.Comments.Trim();
    }

    private static SenStatusHistoryResponse MapStatusHistory(SenStatusHistory h) => new()
    {
        Id = h.Id,
        SenStatusId = h.SenStatusId,
        StartDate = h.StartDate,
        EndDate = h.EndDate
    };

    private static SenNeedResponse MapNeed(StudentSenNeed n) => new()
    {
        Id = n.Id,
        SenTypeId = n.SenTypeId,
        Description = n.Description,
        StartDate = n.StartDate,
        EndDate = n.EndDate,
        Rank = n.Rank
    };

    private static SenProvisionResponse MapProvision(SenProvision p) => new()
    {
        Id = p.Id,
        SenProvisionTypeId = p.SenProvisionTypeId,
        StartDate = p.StartDate,
        EndDate = p.EndDate,
        Frequency = p.Frequency,
        Cost = p.Cost,
        Note = p.Note
    };

    private static SenStatementResponse MapStatement(SenStatement s) => new()
    {
        Id = s.Id,
        IsEhcp = s.IsEhcp,
        AssessmentRequestDate = s.AssessmentRequestDate,
        ParentConsultDate = s.ParentConsultDate,
        FinalisedDate = s.FinalisedDate,
        CeasedDate = s.CeasedDate,
        StatutoryAssessmentAgreedId = s.StatutoryAssessmentAgreedId,
        StatutoryAssessmentOutcomeId = s.StatutoryAssessmentOutcomeId,
        SubjectToTribunal = s.SubjectToTribunal,
        UndergoingMediation = s.UndergoingMediation,
        AppealNotes = s.AppealNotes,
        TemporaryDisapplicationSubjects = s.TemporaryDisapplicationSubjects,
        PermanentDisapplicationSubjects = s.PermanentDisapplicationSubjects,
        LocalAuthorityId = s.LocalAuthorityId,
        Comments = s.Comments
    };

    private async Task<Core.Entities.Student> LoadAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdAsync(studentId, cancellationToken);

        if (student == null)
        {
            throw new NotFoundException("Student not found.");
        }

        return student;
    }
}
