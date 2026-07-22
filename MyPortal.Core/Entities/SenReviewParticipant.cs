using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SenReviewParticipants")]
    public class SenReviewParticipant : Entity
    {
        public Guid SenReviewId { get; set; }

        public Guid PersonId { get; set; }

        public bool Invited { get; set; }

        public bool Attended { get; set; }

        public SenReview? SenReview { get; set; }
        public Person? Person { get; set; }
    }
}
