using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("CurriculumBands")]
    public class CurriculumBand : Entity
    {
        public Guid AcademicYearId { get; set; }

        public Guid CurriculumYearGroupId { get; set; }

        public Guid StudentGroupId { get; set; }

        public AcademicYear? AcademicYear { get; set; }
        public CurriculumYearGroup? CurriculumYearGroup { get; set; }
        public StudentGroup? StudentGroup { get; set; }
    }
}