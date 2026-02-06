using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("CurriculumBands")]
    public class CurriculumBand : Entity, IAcademicYearEntity
    {
        public Guid AcademicYearId { get; set; }

        public Guid CurriculumYearGroupId { get; set; }

        public Guid StudentGroupId { get; set; }

        public AcademicYear? AcademicYear { get; set; }
        public CurriculumYearGroup? CurriculumYearGroup { get; set; }
        public StudentGroup? StudentGroup { get; set; }
    }
}