using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("AcademicTerms")]
    public class AcademicTerm : Entity
    {
        public Guid AcademicYearId { get; set; }
        
        [Required]
        [StringLength(128)]
        public required string Name { get; set; }   

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public AcademicYear? AcademicYear { get; set; }
    }
}