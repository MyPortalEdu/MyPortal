using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Common.Enums;

namespace MyPortal.Core.Entities
{
    [Table("ExamAssessments")]
    public class ExamAssessment : Entity
    {
        public Guid ExamBoardId { get; set; }

        public ExamAssessmentType AssessmentType { get; set; }

        public string? InternalTitle { get; set; }

        public string? ExternalTitle { get; set; }

        // Qualification locale, e.g. ENG / WAL — distinguishes English vs Welsh quals.
        [StringLength(3)]
        public string? Locale { get; set; }

        public ExamBoard? ExamBoard { get; set; }
    }
}