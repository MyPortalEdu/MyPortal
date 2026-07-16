using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class TrainingCertificateRepository : EntityRepository<TrainingCertificate>, ITrainingCertificateRepository
{
    public TrainingCertificateRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) :
        base(factory, authorizationService)
    {
    }

    public async Task<IEnumerable<TrainingCertificate>> GetByStaffMemberIdAsync(Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        // Lean record table (no audit / soft-delete); rows are hard-deleted on reconcile.
        const string sql =
            "SELECT [Id], [TrainingCourseId], [StaffMemberId], [TrainingCertificateStatusId], [CompletedDate], " +
            "[ExpiryDate], [Provider], [Hours], [CertificateReference] FROM [dbo].[TrainingCertificates] " +
            "WHERE [StaffMemberId] = @staffMemberId;";

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition(sql, new { staffMemberId }, transaction,
                cancellationToken: cancellationToken);

            return await conn.QueryAsync<TrainingCertificate>(command);
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
