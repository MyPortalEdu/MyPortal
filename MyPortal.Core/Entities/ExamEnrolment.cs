using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamEnrolments")]
    public class ExamEnrolment : Entity
    {
        public Guid AwardId { get; set; }

        public Guid CandidateId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? RegistrationNumber { get; set; }

        public ExamAward? Award { get; set; }
        public ExamCandidate? Candidate { get; set; }
    }
}