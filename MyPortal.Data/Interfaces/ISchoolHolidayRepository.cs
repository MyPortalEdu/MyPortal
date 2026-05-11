using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Interfaces;

public interface ISchoolHolidayRepository : IEntityRepository<SchoolHoliday>
{
    // Deletes both the SchoolHoliday rows and the backing DiaryEvent rows. The two are
    // created together by AcademicYearService and have no other producers, so cleaning them
    // up in one call avoids leaving orphan DiaryEvents behind on regen.
    Task DeleteByAcademicYearAsync(Guid academicYearId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
