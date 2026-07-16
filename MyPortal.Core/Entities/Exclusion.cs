using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Exclusions")]
    public class Exclusion : Entity, ISoftDeleteEntity
    {
        public Guid StudentId { get; set; }

        public Guid ExclusionTypeId { get; set; }

        public Guid ExclusionReasonId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        // Suspension length in half-day sessions (DfE suspension/exclusion return).
        public int? SessionCount { get; set; }

        // Date the pupil was reinstated following a review (distinct from EndDate).
        public DateTime? ReinstatementDate { get; set; }

        // Body that reviewed the exclusion — governing board vs IRP (CBDS CS137).
        public Guid? ReviewCategoryId { get; set; }

        public string? Comments { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? AppealDate { get; set; }
        
        public DateTime? AppealResultDate { get; set; }

        public Guid? AppealResultId { get; set; }

        public Student? Student { get; set; }
        public ExclusionType? ExclusionType { get; set; }
        public ExclusionReason? ExclusionReason { get; set; }
        public ExclusionAppealResult? AppealResult { get; set; }
        public ExclusionReviewCategory? ReviewCategory { get; set; }
    }
}