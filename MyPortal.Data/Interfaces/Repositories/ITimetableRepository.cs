using MyPortal.Core.Entities;
using MyPortal.Core.Enums;
using MyPortal.Data.Interfaces.Repositories.Base;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Interfaces.Repositories;

public interface ITimetableRepository : IEntityRepository<Timetable>
{
    Task<IList<Timetable>> ListByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken);

    Task<Timetable?> FindActiveAsync(Guid academicYearId, CancellationToken cancellationToken);

    Task<IList<TimetableRun>> ListRunsAsync(Guid timetableId, CancellationToken cancellationToken);

    Task<IList<TimetableAssignment>> ListAssignmentsAsync(Guid timetableId, CancellationToken cancellationToken);

    /// Atomic apply: in one transaction, supersede the previous Active timetable for this
    /// academic year (set EffectiveTo, status -> Superseded) and promote @timetableId to Active
    /// with the supplied effective window.
    Task ApplyAsync(Guid timetableId, Guid academicYearId,
        DateTime effectiveFrom, DateTime? effectiveTo, CancellationToken cancellationToken);

    Task UpdateStatusAsync(Guid timetableId, TimetableStatus status, CancellationToken cancellationToken);
}
