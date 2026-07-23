using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ServiceTerms")]
    public class ServiceTerm : LookupEntity
    {
        [Required]
        [StringLength(16)]
        public string Code { get; set; } = null!;

        public bool IsTeacher { get; set; }

        public bool Salaried { get; set; } = true;

        public bool SpinalProgression { get; set; }

        public bool SinglePaySpine { get; set; }

        public bool TermTimeOnlyPossible { get; set; }

        public byte? IncrementMonth { get; set; }

        public byte? IncrementDay { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? MinimumPoint { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? MaximumPoint { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? PointInterval { get; set; }

        // Pre-filled onto a new contract for this term, then overtypeable.
        [Column(TypeName = "decimal(5,2)")]
        public decimal? HoursPerWeek { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? WeeksPerYear { get; set; }
    }
}
