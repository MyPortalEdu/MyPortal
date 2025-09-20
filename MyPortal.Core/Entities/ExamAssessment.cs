using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Enums;

namespace MyPortal.Core.Entities
{
    [Table("ExamAssessments")]
    public class ExamAssessment : Entity
    {
        public Guid ExamBoardId { get; set; }

        public ExamAssessmentType AssessmentType { get; set; }

        public string? InternalTitle { get; set; }

        public string? ExternalTitle { get; set; }

        public ExamBoard? ExamBoard { get; set; }
    }
}