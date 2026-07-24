using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class TrainingCourseRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<TrainingCourse>(factory, authorizationService), ITrainingCourseRepository
{
    public async Task<IReadOnlyList<TrainingCourseResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT tc.[Id], tc.[Code], tc.[Name], tc.[Description], tc.[Active],
    CAST(CASE WHEN EXISTS (SELECT 1 FROM [dbo].[TrainingEvents] te WHERE te.[TrainingCourseId] = tc.[Id])
              OR EXISTS (SELECT 1 FROM [dbo].[TrainingCertificates] c WHERE c.[TrainingCourseId] = tc.[Id])
         THEN 1 ELSE 0 END AS bit) AS [InUse]
FROM [dbo].[TrainingCourses] tc
ORDER BY tc.[Name];";

        var (conn, owns) = AcquireConnection(null);
        try
        {
            var rows = await conn.QueryAsync<TrainingCourseResponse>(
                new CommandDefinition(sql, cancellationToken: cancellationToken));
            return rows.AsList();
        }
        finally { if (owns) conn.Dispose(); }
    }

    public async Task<bool> IsReferencedAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT CASE WHEN EXISTS (SELECT 1 FROM [dbo].[TrainingEvents] te WHERE te.[TrainingCourseId] = @id)
            OR EXISTS (SELECT 1 FROM [dbo].[TrainingCertificates] c WHERE c.[TrainingCourseId] = @id)
       THEN 1 ELSE 0 END;";

        var (conn, owns) = AcquireConnection(null);
        try
        {
            return await conn.ExecuteScalarAsync<bool>(
                new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken));
        }
        finally { if (owns) conn.Dispose(); }
    }
}
