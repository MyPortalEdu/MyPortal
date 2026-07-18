using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Repositories;

public class TimetableRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<Timetable>(factory, authorizationService), ITimetableRepository
{
    public async Task<IList<Timetable>> ListByAcademicYearAsync(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        var rows = await conn.ExecuteStoredProcedureAsync<Timetable>(
            "[dbo].[usp_timetable_list_by_academic_year]",
            new { academicYearId }, cancellationToken: cancellationToken);
        return rows.ToList();
    }

    public async Task<Timetable?> FindActiveAsync(Guid academicYearId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        var rows = await conn.ExecuteStoredProcedureAsync<Timetable>(
            "[dbo].[usp_timetable_find_active]",
            new { academicYearId, activeStatus = (int)TimetableStatus.Active },
            cancellationToken: cancellationToken);
        return rows.FirstOrDefault();
    }

    public async Task<IList<TimetableRun>> ListRunsAsync(Guid timetableId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        var rows = await conn.ExecuteStoredProcedureAsync<TimetableRun>(
            "[dbo].[usp_timetable_list_runs]",
            new { timetableId }, cancellationToken: cancellationToken);
        return rows.ToList();
    }

    public async Task<IList<TimetableAssignment>> ListAssignmentsAsync(Guid timetableId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            var rows = await conn.ExecuteStoredProcedureAsync<TimetableAssignment>(
                "[dbo].[usp_timetable_list_assignments]",
                new { timetableId }, transaction, cancellationToken: cancellationToken);
            return rows.ToList();
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    public async Task ApplyAsync(Guid timetableId, Guid academicYearId,
        DateTime effectiveFrom, DateTime? effectiveTo, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        // When called inside a UoW the caller's transaction owns commit/rollback; otherwise we
        // open our own connection and transaction so the multi-statement apply stays atomic.
        var (conn, owns) = AcquireConnection(transaction);
        IDbTransaction activeTx;
        bool ownsTransaction;

        if (transaction is null)
        {
            activeTx = conn.BeginTransaction(IsolationLevel.ReadCommitted);
            ownsTransaction = true;
        }
        else
        {
            activeTx = transaction;
            ownsTransaction = false;
        }

        try
        {
            await conn.ExecuteStoredProcedureAsync<int>(
                "[dbo].[usp_timetable_apply]",
                new
                {
                    timetableId,
                    academicYearId,
                    effectiveFrom,
                    effectiveTo,
                    active = (int)TimetableStatus.Active,
                    superseded = (int)TimetableStatus.Superseded,
                }, activeTx, cancellationToken: cancellationToken);

            if (ownsTransaction)
            {
                activeTx.Commit();
            }
        }
        catch
        {
            if (ownsTransaction)
            {
                try { activeTx.Rollback(); } catch { /* nothing useful to do */ }
            }
            throw;
        }
        finally
        {
            if (ownsTransaction) activeTx.Dispose();
            if (owns) conn.Dispose();
        }
    }

    public async Task UpdateStatusAsync(Guid timetableId, TimetableStatus status,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        // SP returns no result set; discardable element type.
        await conn.ExecuteStoredProcedureAsync<int>(
            "[dbo].[usp_timetable_update_status]",
            new { timetableId, status = (int)status },
            cancellationToken: cancellationToken);
    }

    public async Task<IList<AttendancePeriod>> GetAttendancePeriodsForAssignmentsAsync(Guid timetableId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            // Pull every period in the timetable's AcademicYear — materialisation needs the full
            // set in order to walk consecutive periods within a day.
            var rows = await conn.ExecuteStoredProcedureAsync<AttendancePeriod>(
                "[dbo].[usp_timetable_get_attendance_periods_for_assignments]",
                new { timetableId }, transaction, cancellationToken: cancellationToken);
            return rows.ToList();
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    public async Task BulkInsertSessionsAsync(IReadOnlyList<Session> sessions,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        if (sessions.Count == 0) return;

        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            await conn.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO dbo.Sessions
                    (Id, ClassId, TeacherId, RoomId, TimetableId, StartDate, EndDate)
                  VALUES
                    (@Id, @ClassId, @TeacherId, @RoomId, @TimetableId, @StartDate, @EndDate);",
                sessions, transaction: transaction, cancellationToken: cancellationToken));
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    public async Task BulkInsertSessionPeriodsAsync(IReadOnlyList<SessionPeriod> sessionPeriods,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        if (sessionPeriods.Count == 0) return;

        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            await conn.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO dbo.SessionPeriods
                    (Id, SessionId, PeriodId)
                  VALUES
                    (@Id, @SessionId, @PeriodId);",
                sessionPeriods, transaction: transaction, cancellationToken: cancellationToken));
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    public async Task InsertPinAsync(TimetablePin pin, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        // SP returns no result set; discardable element type.
        await conn.ExecuteStoredProcedureAsync<int>(
            "[dbo].[usp_timetable_pin_add]",
            pin, cancellationToken: cancellationToken);
    }

    public async Task<IList<TimetablePin>> ListPinsAsync(Guid timetableId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        var rows = await conn.ExecuteStoredProcedureAsync<TimetablePin>(
            "[dbo].[usp_timetable_pin_list_by_timetable_id]",
            new { timetableId }, cancellationToken: cancellationToken);
        return rows.ToList();
    }

    public async Task<TimetablePin?> GetPinAsync(Guid pinId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        var rows = await conn.ExecuteStoredProcedureAsync<TimetablePin>(
            "[dbo].[usp_timetable_pin_get_by_id]",
            new { pinId }, cancellationToken: cancellationToken);
        return rows.FirstOrDefault();
    }

    public async Task DeletePinAsync(Guid pinId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        // SP returns no result set; discardable element type.
        await conn.ExecuteStoredProcedureAsync<int>(
            "[dbo].[usp_timetable_pin_delete_by_id]",
            new { pinId }, cancellationToken: cancellationToken);
    }

    public async Task<IList<StaffMember>> GetAssignedTeachersAsync(Guid timetableId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            var rows = await conn.ExecuteStoredProcedureAsync<StaffMember>(
                "[dbo].[usp_timetable_get_assigned_teachers]",
                new { timetableId }, transaction, cancellationToken: cancellationToken);
            return rows.ToList();
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    public async Task BulkInsertNonContactAllocationsAsync(
        IReadOnlyList<StaffNonContactAllocation> allocations, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        if (allocations.Count == 0) return;

        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            await conn.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO dbo.StaffNonContactAllocations
                    (Id, StaffMemberId, TimetableId, AttendancePeriodId, Code, StartDate, EndDate)
                  VALUES
                    (@Id, @StaffMemberId, @TimetableId, @AttendancePeriodId, @Code, @StartDate, @EndDate);",
                allocations, transaction: transaction, cancellationToken: cancellationToken));
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

}
