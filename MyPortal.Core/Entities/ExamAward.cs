using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamAwards")]
    public class ExamAward : Entity
    {
        public Guid QualificationId { get; set; }

        public Guid AssessmentId { get; set; }

        public Guid? CourseId { get; set; }

        public string? Description { get; set; }

        public string? AwardCode { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public ExamAssessment? Assessment { get; set; }
        public ExamQualification? Qualification { get; set; }
        public Course? Course { get; set; }
    }
}