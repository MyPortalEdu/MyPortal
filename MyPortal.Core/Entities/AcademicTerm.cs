using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("AcademicTerms")]
    public class AcademicTerm : Entity, IAcademicYearEntity
    {
        public Guid AcademicYearId { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; } = null!;   

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public AcademicYear? AcademicYear { get; set; }
    }
}