using MyPortal.Contracts.Models.Timetabler;

namespace MyPortal.Services.Interfaces.Timetable;

public interface ITimetableService
{
    Task<Guid> CreateDraftAsync(TimetableUpsertRequest model, CancellationToken cancellationToken);

    Task<TimetableDetailsResponse> GetByIdAsync(Guid timetableId, CancellationToken cancellationToken);

    Task<IList<TimetableSummaryResponse>> ListByAcademicYearAsync(Guid academicYearId,
        CancellationToken cancellationToken);

    Task<IList<TimetableRunResponse>> ListRunsAsync(Guid timetableId, CancellationToken cancellationToken);

    Task<TimetableRunResponse> GetRunAsync(Guid runId, CancellationToken cancellationToken);

    Task<IList<TimetableAssignmentResponse>> ListAssignmentsAsync(Guid timetableId,
        CancellationToken cancellationToken);

    Task ApplyAsync(Guid timetableId, TimetableApplyRequest model, CancellationToken cancellationToken);

    Task DiscardAsync(Guid timetableId, CancellationToken cancellationToken);

    Task<Guid> AddPinAsync(Guid timetableId, TimetablePinUpsertRequest model,
        CancellationToken cancellationToken);

    Task<IList<TimetablePinResponse>> ListPinsAsync(Guid timetableId, CancellationToken cancellationToken);

    Task RemovePinAsync(Guid timetableId, Guid pinId, CancellationToken cancellationToken);
}
