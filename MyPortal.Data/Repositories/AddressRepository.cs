using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class AddressRepository : EntityRepository<Address>, IAddressRepository
{
    public AddressRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) : base(
        factory, authorizationService)
    {
    }

    public async Task<IReadOnlyList<PersonAddressResponse>> GetByPersonIdAsync(Guid personId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition("[dbo].[usp_address_get_by_person_id]",
                new { personId }, transaction, commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var rows = await conn.QueryAsync<PersonAddressResponse>(command);
            return rows.ToList();
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<IReadOnlyList<AddressMatchResponse>> SearchAsync(string like,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition("[dbo].[usp_address_search]",
                new { like }, transaction, commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var rows = await conn.QueryAsync<AddressMatchResponse>(command);
            return rows.ToList();
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<Address?> FindByMatchKeyAsync(string postcode, string? buildingNumber,
        string? buildingName, string street, CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition("[dbo].[usp_address_find_by_match_key]",
                new { postcode, buildingNumber, buildingName, street }, transaction,
                commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

            return await conn.QueryFirstOrDefaultAsync<Address>(command);
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
