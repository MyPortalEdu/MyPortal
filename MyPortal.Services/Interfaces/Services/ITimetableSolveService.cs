using MyPortal.Core.Entities;

namespace MyPortal.Services.Interfaces.Services;

public interface ITimetableSolveService
{
    /// Loads the timetable's curriculum/staff/room/pin context, builds a solver input,
    /// runs CP-SAT, and persists the resulting assignments. Synchronous; expected to be
    /// fronted by a job queue once async wrapping lands.
    Task<TimetableRun> RunAsync(Guid timetableId, Guid weekPatternId,
        CancellationToken cancellationToken);
}
