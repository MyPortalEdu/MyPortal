using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SchoolHolidays")]
    public class SchoolHoliday : Entity
    {
        public Guid EventId { get; set; }

        public Guid AcademicYearId { get; set; }

        public DiaryEvent? Event { get; set; }
        public AcademicYear? AcademicYear { get; set; }
    }
}
