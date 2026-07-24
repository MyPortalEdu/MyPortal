using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

public interface IStaffIncrementService
{
    /// <summary>
    /// Model the annual increment for a service term without writing anything: who moves up a
    /// point, to what, and what their salary becomes.
    /// </summary>
    Task<IncrementPreviewResponse> PreviewAsync(Guid serviceTermId, IncrementPreviewRequest model,
        CancellationToken cancellationToken);

    /// <summary>Apply the increment now to the chosen contracts, recording each move in salary history.</summary>
    Task ApplyAsync(Guid serviceTermId, IncrementApplyRequest model, CancellationToken cancellationToken);

    /// <summary>Schedule an increment for a future date; applied when due, re-computed at that point.</summary>
    Task<Guid> ScheduleAsync(Guid serviceTermId, IncrementScheduleRequest model,
        CancellationToken cancellationToken);

    /// <summary>Every scheduled increment for a service term (pending and past).</summary>
    Task<IReadOnlyList<ScheduledIncrementResponse>> GetScheduledAsync(Guid serviceTermId,
        CancellationToken cancellationToken);

    /// <summary>Pending increments whose date has arrived, across all terms — the due worklist.</summary>
    Task<IReadOnlyList<ScheduledIncrementResponse>> GetDueAsync(CancellationToken cancellationToken);

    /// <summary>Cancel a pending scheduled increment.</summary>
    Task CancelScheduledAsync(Guid scheduledId, CancellationToken cancellationToken);

    /// <summary>Run a scheduled increment now, applying to everyone eligible and marking it complete.</summary>
    Task ApplyScheduledAsync(Guid scheduledId, CancellationToken cancellationToken);
}
