using MyPortal.Core.Entities;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Interfaces;

public interface IStudentGroupSupervisorRepository : IBaseEntityRepository<StudentGroupSupervisor, Guid>
{
    Task<IList<StudentGroupSupervisor>> GetStudentGroupSupervisorsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);
}
