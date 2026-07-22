using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Models;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;
using QueryKit.Extensions;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Repositories;

public class ContactRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<Contact>(factory, authorizationService), IContactRepository
{
    public async Task<PageResult<ContactSummaryResponse>> GetContactsAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        var sql = SqlResourceLoader.Load("People.GetContactSummaries.sql");

        return await GetListPagedAsync<ContactSummaryResponse>(sql, null, filter, sort, paging, false,
            cancellationToken);
    }

    public async Task<ContactHeaderRow?> GetHeaderByIdAsync(Guid contactId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition("[dbo].[usp_contact_get_header_by_id]", new { contactId },
                transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

            return await conn.QueryFirstOrDefaultAsync<ContactHeaderRow>(command);
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<ContactBasicDetailsResponse?> GetBasicDetailsByIdAsync(Guid contactId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition("[dbo].[usp_contact_get_basic_details_by_id]", new { contactId },
                transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

            return await conn.QueryFirstOrDefaultAsync<ContactBasicDetailsResponse>(command);
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<Guid?> GetContactIdByPersonIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var rows = await conn.ExecuteStoredProcedureAsync<Guid?>(
                "[dbo].[usp_contact_get_id_by_person_id]", new { personId }, transaction,
                cancellationToken: cancellationToken);

            return rows.FirstOrDefault();
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<IReadOnlyList<ContactMatchResponse>> SearchPeopleForContactCreateAsync(string like,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var sql = SqlResourceLoader.Load("People.SearchPeopleForContactCreate.sql");

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { like }, transaction,
                cancellationToken: cancellationToken);

            var rows = await conn.QueryAsync<ContactMatchResponse>(command);

            return rows.AsList();
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
