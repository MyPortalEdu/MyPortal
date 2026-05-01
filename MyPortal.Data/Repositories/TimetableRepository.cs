using System.Transactions;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Core.Enums;
using MyPortal.Data.Interfaces.Repositories;
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
        CancellationToken cancellationToken)
    {
        using var conn = _factory.Create();
        var rows = await conn.QueryAsync<TimetableAssignment>(new CommandDefinition(
            "SELECT * FROM dbo.TimetableAssignments WHERE TimetableId = @timetableId;",
            new { timetableId }, cancellationToken: cancellationToken));
        return rows.ToList();
    }

    public async Task ApplyAsync(Guid timetableId, Guid academicYearId,
        DateTime effectiveFrom, DateTime? effectiveTo, CancellationToken cancellationToken)
    {
        using var tx = new TransactionScope(TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        using var conn = _factory.Create();

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
            }, cancellationToken: cancellationToken));

        await conn.ExecuteAsync(new CommandDefinition(
            @"UPDATE dbo.Timetables
                  SET Status = @active,
                      EffectiveFrom = @effectiveFrom,
                      EffectiveTo = @effectiveTo,
                      LastModifiedAt = SYSUTCDATETIME()
                WHERE Id = @timetableId;",
            new { timetableId, active = (int)TimetableStatus.Active, effectiveFrom, effectiveTo },
            cancellationToken: cancellationToken));

        tx.Complete();
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
}
