using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // A future annual increment for a service term. Not a frozen list of moves: eligibility is
    // re-computed when it is applied, so it reflects reality on the day, not on the day it was set up.
    [Table("ScheduledIncrements")]
    public class ScheduledIncrement : Entity, IAuditableEntity
    {
        public Guid ServiceTermId { get; set; }

        public DateTime EffectiveDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Scheduled";

        public DateTime? CompletedAt { get; set; }

        public int? AppliedCount { get; set; }

        public ServiceTerm? ServiceTerm { get; set; }

        public Guid CreatedById { get; set; }
        public string CreatedByIpAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Guid LastModifiedById { get; set; }
        public string LastModifiedByIpAddress { get; set; } = string.Empty;
        public DateTime LastModifiedAt { get; set; }
        public User? CreatedBy { get; set; }
        public User? LastModifiedBy { get; set; }
    }
}
