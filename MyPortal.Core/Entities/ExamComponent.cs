using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamComponents")]
    public class ExamComponent : Entity
    {
        public Guid BaseComponentId { get; set; }

        public Guid ExamSeriesId { get; set; }

        public Guid AssessmentModeId { get; set; }

        // Components can be COURSEWORK or EXAMINATIONS
        public Guid? ExamDateId { get; set; }

        public DateTime? DateDue { get; set; }

        public DateTime? DateSubmitted { get; set; }

        public int MaximumMark { get; set; }

        public ExamBaseComponent? BaseComponent { get; set; }
        public ExamSeries? Series { get; set; }
        public ExamAssessmentMode? AssessmentMode { get; set; } 
        public ExamDate? ExamDate { get; set; }
    }
}