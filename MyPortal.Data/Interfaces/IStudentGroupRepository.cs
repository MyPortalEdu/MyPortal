using MyPortal.Core.Entities;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Interfaces;

public interface IStudentGroupRepository : IBaseEntityRepository<StudentGroup, Guid>
{
    Task<IList<StudentGroup>> GetStudentGroupsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);
}
