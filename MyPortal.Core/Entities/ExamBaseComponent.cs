using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamBaseComponents")]
    public class ExamBaseComponent : Entity
    {
        public Guid AssessmentModeId { get; set; }

        public Guid ExamAssessmentId { get; set; }

        public string? ComponentCode { get; set; }

        public ExamAssessmentMode? AssessmentMode { get; set; }
        public ExamAssessment? Assessment { get; set; }
    }
}