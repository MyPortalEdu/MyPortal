using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;

namespace MyPortal.Data.Repositories;

public class SwfCensusRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StaffMember>(factory, authorizationService), ISwfCensusRepository
{
    public async Task<SwfCensusHeaderRow?> GetHeaderAsync(CancellationToken cancellationToken)
    {
        var sql = SqlResourceLoader.Load("People.GetSwfCensusHeader.sql");
        var (conn, owns) = AcquireConnection(null);
        try
        {
            return await conn.QuerySingleOrDefaultAsync<SwfCensusHeaderRow>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));
        }
        finally { if (owns) conn.Dispose(); }
    }

    public Task<IReadOnlyList<SwfCensusMemberRow>> GetMembersAsync(DateTime referenceDate,
        CancellationToken cancellationToken) =>
        QueryAsync<SwfCensusMemberRow>("People.GetSwfCensusMembers.sql", new { referenceDate },
            cancellationToken);

    public Task<IReadOnlyList<SwfCensusAbsenceRow>> GetAbsencesAsync(DateTime absenceFrom, DateTime absenceTo,
        CancellationToken cancellationToken) =>
        QueryAsync<SwfCensusAbsenceRow>("People.GetSwfCensusAbsences.sql", new { absenceFrom, absenceTo },
            cancellationToken);

    public Task<IReadOnlyList<SwfCensusAllowanceRow>> GetAllowancesAsync(DateTime referenceDate,
        CancellationToken cancellationToken) =>
        QueryAsync<SwfCensusAllowanceRow>("People.GetSwfCensusAllowances.sql", new { referenceDate },
            cancellationToken);

    private async Task<IReadOnlyList<T>> QueryAsync<T>(string resource, object parameters,
        CancellationToken cancellationToken)
    {
        var sql = SqlResourceLoader.Load(resource);
        var (conn, owns) = AcquireConnection(null);
        try
        {
            var rows = await conn.QueryAsync<T>(
                new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
            return rows.AsList();
        }
        finally { if (owns) conn.Dispose(); }
    }
}
