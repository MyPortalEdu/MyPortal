using System.Data;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Parameters;
using MyPortal.Data.Repositories.Base;
using QueryKit.Extensions;

namespace MyPortal.Data.Repositories;

public class StaffAbsenceCertificateRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StaffAbsenceCertificate>(factory, authorizationService), IStaffAbsenceCertificateRepository
{
    public async Task<IEnumerable<StaffAbsenceCertificate>> GetByAbsenceIdsAsync(IEnumerable<Guid> absenceIds,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var ids = absenceIds.ToList();

        if (ids.Count == 0)
        {
            return Enumerable.Empty<StaffAbsenceCertificate>();
        }

        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            return await conn.ExecuteStoredProcedureAsync<StaffAbsenceCertificate>(
                "[dbo].[usp_staff_absence_certificate_get_by_absence_ids]",
                new { absenceIds = ids.ToGuidTvp() }, transaction,
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
