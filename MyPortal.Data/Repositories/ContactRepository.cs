using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class ContactRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<Contact>(factory, authorizationService), IContactRepository
{
    public async Task<Contact?> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null)
    {
        const string sql =
            "SELECT [Id], [PersonId], [ParentalBallot], [PlaceOfWork], [JobTitle], [NiNumber] " +
            "FROM [dbo].[Contacts] WHERE [PersonId] = @personId;";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { personId }, transaction,
                cancellationToken: cancellationToken);

            return await conn.QueryFirstOrDefaultAsync<Contact>(command);
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
