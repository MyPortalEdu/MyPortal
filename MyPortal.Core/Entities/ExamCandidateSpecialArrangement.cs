using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamCandidateSpecialArrangements")]
    public class ExamCandidateSpecialArrangement : Entity
    {
        public Guid CandidateId { get; set; }

        public Guid SpecialArrangementId { get; set; }

        public ExamCandidate? Candidate { get; set; }
        public ExamSpecialArrangement? SpecialArrangement { get; set; }
    }
}