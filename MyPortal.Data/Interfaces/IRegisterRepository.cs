using MyPortal.Contracts.Models.Attendance;

namespace MyPortal.Data.Interfaces;

public interface IRegisterRepository
{
    Task<RegisterResponse?> GetLessonRegisterAsync(Guid sessionPeriodId, Guid attendanceWeekId,
        CancellationToken cancellationToken);

    Task<RegisterResponse?> GetRegGroupRegisterAsync(Guid regGroupId, Guid attendancePeriodId,
        Guid attendanceWeekId, CancellationToken cancellationToken);

    Task SubmitLessonRegisterAsync(Guid sessionPeriodId, Guid attendanceWeekId,
        IReadOnlyCollection<SubmitMarkRequest> marks, CancellationToken cancellationToken);

    Task SubmitRegGroupRegisterAsync(Guid regGroupId, Guid attendancePeriodId, Guid attendanceWeekId,
        IReadOnlyCollection<SubmitMarkRequest> marks, CancellationToken cancellationToken);
}
