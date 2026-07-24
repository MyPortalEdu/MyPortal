using System.Data;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Interfaces;

public interface ITrainingEventRepository : IEntityRepository<TrainingEvent>
{
    Task<IReadOnlyList<TrainingEventSummaryResponse>> GetSummariesAsync(DateTime from, DateTime to,
        CancellationToken cancellationToken);

    Task<TrainingEventDetailsResponse?> GetDetailsAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TrainingEventAttendeeResponse>> GetAttendeesAsync(Guid diaryEventId,
        CancellationToken cancellationToken);

    Task BookAttendeeAsync(Guid diaryEventId, Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    Task RemoveAttendeeAsync(Guid diaryEventId, Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    Task SetAttendedAsync(Guid diaryEventId, Guid personId, bool attended, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    Task EnsureAttendanceCertificateAsync(Guid trainingEventId, Guid staffMemberId, Guid trainingCourseId,
        Guid statusId, DateTime completedDate, decimal? hours, string? provider,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    Task DeleteAttendanceCertificateAsync(Guid trainingEventId, Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    Task PurgeAsync(Guid trainingEventId, Guid diaryEventId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
