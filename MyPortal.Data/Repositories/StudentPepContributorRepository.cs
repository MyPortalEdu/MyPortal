using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class StudentPepContributorRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StudentPepContributor>(factory, authorizationService), IStudentPepContributorRepository
{
    public async Task<IEnumerable<PepContributorResponse>> GetByPepIdAsync(Guid studentPepId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<PepContributorResponse>(
                "[dbo].[usp_student_pep_contributor_get_by_pep_id]", new { studentPepId }, transaction,
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
