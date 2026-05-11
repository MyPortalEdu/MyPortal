using System.Data;
using MyPortal.Common.Enums;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Interfaces;

public interface ITimetableRepository : IEntityRepository<Timetable>
{
    Task<IList<Timetable>> ListByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken);

    Task<Timetable?> FindActiveAsync(Guid academicYearId, CancellationToken cancellationToken);

    Task<IList<TimetableRun>> ListRunsAsync(Guid timetableId, CancellationToken cancellationToken);

    Task<IList<TimetableAssignment>> ListAssignmentsAsync(Guid timetableId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    /// Atomic apply: in one transaction, supersede the previous Active timetable for this
    /// academic year (set EffectiveTo, status -> Superseded) and promote @timetableId to Active
    /// with the supplied effective window.
    Task ApplyAsync(Guid timetableId, Guid academicYearId,
        DateTime effectiveFrom, DateTime? effectiveTo, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    Task UpdateStatusAsync(Guid timetableId, TimetableStatus status, CancellationToken cancellationToken);

    /// Loads every AttendancePeriod in the timetable's AcademicYear. Used by materialisation
    /// to walk consecutive periods for multi-size slots.
    Task<IList<AttendancePeriod>> GetAttendancePeriodsForAssignmentsAsync(Guid timetableId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    Task BulkInsertSessionsAsync(IReadOnlyList<Session> sessions, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    Task BulkInsertSessionPeriodsAsync(IReadOnlyList<SessionPeriod> sessionPeriods,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    Task InsertPinAsync(TimetablePin pin, CancellationToken cancellationToken);

    Task<IList<TimetablePin>> ListPinsAsync(Guid timetableId, CancellationToken cancellationToken);

    Task<TimetablePin?> GetPinAsync(Guid pinId, CancellationToken cancellationToken);

    Task DeletePinAsync(Guid pinId, CancellationToken cancellationToken);

    Task<IList<StaffMember>> GetAssignedTeachersAsync(Guid timetableId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    Task BulkInsertNonContactAllocationsAsync(
        IReadOnlyList<StaffNonContactAllocation> allocations, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
