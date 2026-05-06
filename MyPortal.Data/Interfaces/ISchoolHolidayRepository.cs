using System.Data;
using MyPortal.Core.Entities;
using QueryKit.Repositories.Interfaces;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Interfaces;

public interface ISchoolHolidayRepository : IBaseEntityRepository<SchoolHoliday, Guid>
{
    // Deletes both the SchoolHoliday rows and the backing DiaryEvent rows. The two are
    // created together by AcademicYearService and have no other producers, so cleaning them
    // up in one call avoids leaving orphan DiaryEvents behind on regen.
    Task DeleteByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
