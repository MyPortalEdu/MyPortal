using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class PhoneNumberRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<PhoneNumber>(factory, authorizationService), IPhoneNumberRepository
{
    public async Task<IReadOnlyList<PhoneNumber>> GetByPersonIdAsync(Guid personId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition("[dbo].[usp_phone_number_get_by_person_id]",
                new { personId }, transaction, commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var rows = await conn.QueryAsync<PhoneNumber>(command);
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
}
