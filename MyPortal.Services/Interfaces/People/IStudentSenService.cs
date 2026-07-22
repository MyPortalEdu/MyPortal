using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Students;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// The SEN area of the student profile — the cached current SEN status (denormalised on the Student)
/// plus the dated status history, ranked needs, provisions and statutory statements. Gated on
/// Student.{View|Edit}StudentSen. Reviews are a later increment and are not handled here.
/// </summary>
public interface IStudentSenService
{
    /// <summary>SEN area read — the cached current status and start date, the census flags, the four
    /// child collections and the picker option lists. 404 if the student doesn't exist.</summary>
    Task<StudentSenDetailsResponse> GetSenDetailsAsync(Guid studentId, CancellationToken cancellationToken);

    /// <summary>Set the current SEN status: close the open status-history row, open a new one from the
    /// requested start date, and refresh the cache on the Student. No-op when the status is unchanged.</summary>
    Task SetSenStatusAsync(Guid studentId, SetSenStatusRequest model, CancellationToken cancellationToken);

    /// <summary>Undo the latest SEN status change: remove the current (open) status-history entry, re-open
    /// the previous one as current, and refresh the cache on the Student (cleared if none remain). Closed
    /// history is untouched. No-op when no status is recorded.</summary>
    Task UndoLatestSenStatusAsync(Guid studentId, CancellationToken cancellationToken);

    /// <summary>Reconcile the student's SEN needs to the supplied set.</summary>
    Task UpdateSenNeedsAsync(Guid studentId, IEnumerable<SenNeedUpsertRequest> model,
        CancellationToken cancellationToken);

    /// <summary>Reconcile the student's SEN provisions to the supplied set.</summary>
    Task UpdateSenProvisionsAsync(Guid studentId, IEnumerable<SenProvisionUpsertRequest> model,
        CancellationToken cancellationToken);

    /// <summary>Reconcile the student's statutory SEN statements to the supplied set.</summary>
    Task UpdateSenStatementsAsync(Guid studentId, IEnumerable<SenStatementUpsertRequest> model,
        CancellationToken cancellationToken);
}
