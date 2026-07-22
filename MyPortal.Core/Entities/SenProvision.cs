using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SenProvisions")]
    public class SenProvision : Entity
    {
        public Guid StudentId { get; set; }

        public Guid SenProvisionTypeId { get; set; }

        public DateTime StartDate { get; set; }

        // Null for open-ended provision that has no scheduled end.
        public DateTime? EndDate { get; set; }

        // Free-text delivery cadence, e.g. "3× weekly", "1 hour daily".
        [StringLength(128)]
        public string? Frequency { get; set; }

        // Recorded cost of the provision, where tracked.
        public decimal? Cost { get; set; }

        [Required]
        public string Note { get; set; } = null!;

        public Student? Student { get; set; }

        public SenProvisionType? SenProvisionType { get; set; }
    }
}