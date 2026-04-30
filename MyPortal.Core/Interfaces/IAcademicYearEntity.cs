using MyPortal.Core.Entities;

namespace MyPortal.Core.Interfaces;

public interface IAcademicYearEntity
{
    Guid AcademicYearId { get; set; }
    AcademicYear? AcademicYear { get; set; }
}