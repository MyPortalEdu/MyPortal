using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamAssessmentAspects")]
    public class ExamAssessmentAspect : Entity
    {
        public Guid AssessmentId { get; set; }

        public Guid AspectId { get; set; }

        public Guid SeriesId { get; set; }

        public int AspectOrder { get; set; }

        public Aspect? Aspect { get; set; }
        public ExamAssessment? Assessment { get; set; }
        public ExamSeries? Series { get; set; }
    }
}