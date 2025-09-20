using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SenReviews")]
    public class SenReview : Entity
    {
        public Guid StudentId { get; set; }

        public Guid SenReviewTypeId { get; set; }

        public Guid SenReviewStatusId { get; set; }

        public Guid? SencoId { get; set; }

        public Guid DiaryEventId { get; set; }
        
        // When this gets updated, the student's SEN status should also be updated
        public Guid? OutcomeSenStatusId { get; set; }
        
        [StringLength(256)]
        public string? Comments { get; set; }

        public Student? Student { get; set; }
        public StaffMember? Senco { get; set; }
        public DiaryEvent? DiaryEvent { get; set; }
        public SenStatus? OutcomeSenStatus { get; set; }
        public SenReviewStatus? ReviewStatus { get; set; }
        public SenReviewType? ReviewType { get; set; }
    }
}