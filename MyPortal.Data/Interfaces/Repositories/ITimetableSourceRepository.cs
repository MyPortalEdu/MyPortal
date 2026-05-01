using MyPortal.Data.Timetabler;

namespace MyPortal.Data.Interfaces.Repositories;

public interface ITimetableSourceRepository
{
    /// Hydrates every entity slice the timetabler needs to build solver input for the given
    /// timetable, scoped to the supplied week pattern. The week pattern fixes which
    /// AttendancePeriod set the solver schedules against.
    Task<TimetableInputSources> LoadAsync(Guid timetableId, Guid weekPatternId,
        CancellationToken cancellationToken);
}
