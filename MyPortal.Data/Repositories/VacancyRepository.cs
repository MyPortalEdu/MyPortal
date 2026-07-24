using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Parameters;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class VacancyRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<Vacancy>(factory, authorizationService), IVacancyRepository
{
    public async Task<IEnumerable<Vacancy>> GetByPostIdsAsync(IEnumerable<Guid> postIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var ids = postIds.ToList();

        if (ids.Count == 0)
        {
            return Enumerable.Empty<Vacancy>();
        }

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<Vacancy>(
                "[dbo].[usp_vacancy_get_by_post_ids]", new { postIds = ids.ToGuidTvp() }, transaction,
                cancellationToken: cancellationToken);
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }
}
