using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Repositories;

public class TimetableRepository : EntityRepository<Timetable>, ITimetableRepository
{
    public TimetableRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
        : base(factory, authorizationService)
    {
    }

    public async Task<IList<Timetable>> ListByAcademicYearAsync(Guid academicYearId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<Timetable>(new CommandDefinition(
            @"SELECT * FROM dbo.Timetables
               WHERE AcademicYearId = @academicYearId
               ORDER BY CreatedAt DESC;",
            new { academicYearId }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<Timetable?> FindActiveAsync(Guid academicYearId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<Timetable>(new CommandDefinition(
            @"SELECT TOP 1 * FROM dbo.Timetables
               WHERE AcademicYearId = @academicYearId AND Status = @activeStatus;",
            new { academicYearId, activeStatus = (int)TimetableStatus.Active },
            cancellationToken: cancellationToken));
    }

    public async Task<IList<TimetableRun>> ListRunsAsync(Guid timetableId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<TimetableRun>(new CommandDefinition(
            @"SELECT * FROM dbo.TimetableRuns
               WHERE TimetableId = @timetableId
               ORDER BY StartedAt DESC;",
            new { timetableId }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<IList<TimetableAssignment>> ListAssignmentsAsync(Guid timetableId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            var rows = await conn.QueryAsync<TimetableAssignment>(new CommandDefinition(
                "SELECT * FROM dbo.TimetableAssignments WHERE TimetableId = @timetableId;",
                new { timetableId }, transaction: transaction, cancellationToken: cancellationToken));
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
            // Capture the IDs of timetables we're about to supersede — we need them after the
            // status flip to truncate downstream Sessions / NonContactAllocations.
            var supersededIds = (await conn.QueryAsync<Guid>(new CommandDefinition(
                @"SELECT Id FROM dbo.Timetables
                   WHERE AcademicYearId = @academicYearId
                     AND Status = @active
                     AND Id <> @timetableId;",
                new { academicYearId, timetableId, active = (int)TimetableStatus.Active },
                transaction: activeTx, cancellationToken: cancellationToken))).ToArray();

            // Close out any currently-active timetable for this academic year. EffectiveTo gets the
            // day before applyDate so there's no overlap window where two timetables claim Active.
            await conn.ExecuteAsync(new CommandDefinition(
                @"UPDATE dbo.Timetables
                      SET Status = @superseded,
                          EffectiveTo = DATEADD(DAY, -1, @effectiveFrom),
                          LastModifiedAt = SYSUTCDATETIME()
                    WHERE AcademicYearId = @academicYearId
                      AND Status = @active
                      AND Id <> @timetableId;",
                new
                {
                    academicYearId,
                    timetableId,
                    effectiveFrom,
                    active = (int)TimetableStatus.Active,
                    superseded = (int)TimetableStatus.Superseded,
                }, transaction: activeTx, cancellationToken: cancellationToken));

            // Truncate prior Sessions and NonContactAllocations so the date-range filter in the
            // register query doesn't see two timetables overlapping after the cutover. Only shrink
            // — never extend — and skip rows already ended before applyDate (untouched history).
            if (supersededIds.Length > 0)
            {
                await conn.ExecuteAsync(new CommandDefinition(
                    @"UPDATE dbo.Sessions
                          SET EndDate = DATEADD(DAY, -1, @effectiveFrom)
                        WHERE TimetableId IN @supersededIds
                          AND EndDate >= @effectiveFrom;",
                    new { effectiveFrom, supersededIds },
                    transaction: activeTx, cancellationToken: cancellationToken));

                await conn.ExecuteAsync(new CommandDefinition(
                    @"UPDATE dbo.StaffNonContactAllocations
                          SET EndDate = DATEADD(DAY, -1, @effectiveFrom)
                        WHERE TimetableId IN @supersededIds
                          AND EndDate >= @effectiveFrom;",
                    new { effectiveFrom, supersededIds },
                    transaction: activeTx, cancellationToken: cancellationToken));
            }

            await conn.ExecuteAsync(new CommandDefinition(
                @"UPDATE dbo.Timetables
                      SET Status = @active,
                          EffectiveFrom = @effectiveFrom,
                          EffectiveTo = @effectiveTo,
                          LastModifiedAt = SYSUTCDATETIME()
                    WHERE Id = @timetableId;",
                new { timetableId, active = (int)TimetableStatus.Active, effectiveFrom, effectiveTo },
                transaction: activeTx, cancellationToken: cancellationToken));

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
        await conn.ExecuteAsync(new CommandDefinition(
            @"UPDATE dbo.Timetables
                  SET Status = @status,
                      LastModifiedAt = SYSUTCDATETIME()
                WHERE Id = @timetableId;",
            new { timetableId, status = (int)status },
            cancellationToken: cancellationToken));
    }

    public async Task<IList<AttendancePeriod>> GetAttendancePeriodsForAssignmentsAsync(Guid timetableId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            // Pull every period in the timetable's AcademicYear — materialisation needs the full
            // set in order to walk consecutive periods within a day.
            var rows = await conn.QueryAsync<AttendancePeriod>(new CommandDefinition(
                @"SELECT AP.*
                    FROM dbo.AttendancePeriods AP
                    JOIN dbo.Timetables T ON T.AcademicYearId = AP.AcademicYearId
                   WHERE T.Id = @timetableId;",
                new { timetableId }, transaction: transaction, cancellationToken: cancellationToken));
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
        await conn.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO dbo.TimetablePins
                (Id, TimetableId, CurriculumBlockId, SlotIndex, ClassId, TeacherId, RoomId,
                 StartAttendancePeriodId, CreatedById, CreatedByIpAddress, CreatedAt)
              VALUES
                (@Id, @TimetableId, @CurriculumBlockId, @SlotIndex, @ClassId, @TeacherId, @RoomId,
                 @StartAttendancePeriodId, @CreatedById, @CreatedByIpAddress, @CreatedAt);",
            pin, cancellationToken: cancellationToken));
    }

    public async Task<IList<TimetablePin>> ListPinsAsync(Guid timetableId,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<TimetablePin>(new CommandDefinition(
            @"SELECT * FROM dbo.TimetablePins
               WHERE TimetableId = @timetableId
               ORDER BY CreatedAt;",
            new { timetableId }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task<TimetablePin?> GetPinAsync(Guid pinId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        return await conn.QuerySingleOrDefaultAsync<TimetablePin>(new CommandDefinition(
            "SELECT * FROM dbo.TimetablePins WHERE Id = @pinId;",
            new { pinId }, cancellationToken: cancellationToken));
    }

    public async Task DeletePinAsync(Guid pinId, CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        await conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM dbo.TimetablePins WHERE Id = @pinId;",
            new { pinId }, cancellationToken: cancellationToken));
    }

    public async Task<IList<StaffMember>> GetAssignedTeachersAsync(Guid timetableId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            var rows = await conn.QueryAsync<StaffMember>(new CommandDefinition(
                @"SELECT DISTINCT SM.*
                    FROM dbo.StaffMembers SM
                    JOIN dbo.TimetableAssignments TA ON TA.TeacherId = SM.Id
                    WHERE TA.TimetableId = @timetableId
                      AND SM.IsDeleted = 0;",
                new { timetableId }, transaction: transaction, cancellationToken: cancellationToken));
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
