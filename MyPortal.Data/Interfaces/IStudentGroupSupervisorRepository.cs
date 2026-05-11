using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IStudentGroupSupervisorRepository : IEntityRepository<StudentGroupSupervisor>
{
    Task<IList<StudentGroupSupervisor>> GetStudentGroupSupervisorsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);

    Task<IList<StudentGroupSupervisor>> GetByStudentGroupAsync(Guid studentGroupId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
