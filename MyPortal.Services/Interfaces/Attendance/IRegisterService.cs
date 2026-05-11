using MyPortal.Contracts.Models.Attendance;

namespace MyPortal.Services.Interfaces.Attendance;

public interface IRegisterService
{
    Task<RegisterResponse> GetLessonRegisterAsync(Guid sessionPeriodId, Guid attendanceWeekId,
        CancellationToken cancellationToken);

    Task<RegisterResponse> GetRegGroupRegisterAsync(Guid regGroupId, Guid attendancePeriodId,
        Guid attendanceWeekId, CancellationToken cancellationToken);

    Task SubmitLessonRegisterAsync(Guid sessionPeriodId, Guid attendanceWeekId, SubmitRegisterRequest model,
        CancellationToken cancellationToken);

    Task SubmitRegGroupRegisterAsync(Guid regGroupId, Guid attendancePeriodId, Guid attendanceWeekId,
        SubmitRegisterRequest model, CancellationToken cancellationToken);
}
