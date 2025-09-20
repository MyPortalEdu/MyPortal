using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamCandidateSeries")]
    public class ExamCandidateSeries : Entity
    {
        public Guid SeriesId { get; set; }

        public Guid CandidateId { get; set; }

        public string? Flag { get; set; }

        public ExamSeries? Series { get; set; }
        public ExamCandidate? Candidate { get; set; }
    }
}