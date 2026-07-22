using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class StudentCareEpisodeRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StudentCareEpisode>(factory, authorizationService), IStudentCareEpisodeRepository
{
    public async Task<IEnumerable<StudentCareEpisode>> GetByStudentIdAsync(Guid studentId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<StudentCareEpisode>(
                "[dbo].[usp_student_care_episode_get_by_student_id]", new { studentId }, transaction,
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
