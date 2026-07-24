using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

public interface ITrainingEventService
{
    Task<IReadOnlyList<TrainingEventSummaryResponse>> ListAsync(DateTime? from, DateTime? to,
        CancellationToken cancellationToken);

    Task<TrainingEventDetailsResponse?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<Guid> CreateAsync(TrainingEventUpsertRequest model, CancellationToken cancellationToken);

    Task UpdateAsync(Guid id, TrainingEventUpsertRequest model, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    Task BookAttendeesAsync(Guid id, IEnumerable<Guid> staffMemberIds, CancellationToken cancellationToken);

    Task RemoveAttendeeAsync(Guid id, Guid staffMemberId, CancellationToken cancellationToken);

    Task SetAttendanceAsync(Guid id, Guid staffMemberId, bool attended, CancellationToken cancellationToken);
}
