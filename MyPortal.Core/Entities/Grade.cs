using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Grades")]
    public class Grade : Entity
    {
        public Guid GradeSetId { get; set; }

        [Required]
        [StringLength(25)]
        public string Code { get; set; } = null!;

        [StringLength(50)] 
        public string? Description { get; set; }
        
        public decimal Value { get; set; }

        public GradeSet? GradeSet { get; set; }
    }
}