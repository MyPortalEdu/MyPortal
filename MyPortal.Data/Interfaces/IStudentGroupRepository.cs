using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Interfaces;

public interface IStudentGroupRepository : IEntityRepository<StudentGroup>
{
    Task<IList<StudentGroup>> GetStudentGroupsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);

    // Wipes the entire pastoral hierarchy for the AY (StudentGroups + Supervisors +
    // YearGroups + RegGroups + Houses) in FK-safe order. Caller must have already
    // verified that no membership/marksheet/etc. data references these groups — this
    // method does not check, it just deletes.
    Task DeleteByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
