using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamCandidate")]
    public class ExamCandidate : Entity
    {
        public Guid StudentId { get; set; }

        public string? Uci { get; set; }

        [StringLength(4)] 
        public string? CandidateNumber { get; set; }

        [StringLength(4)] 
        public string? PreviousCandidateNumber { get; set; }

        [StringLength(5)] 
        public string? PreviousCentreNumber { get; set; }

        public bool SpecialConsideration { get; set; }

        public string? Note { get; set; }

        public Student? Student { get; set; }
    }
}