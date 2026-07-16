using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamBaseElements")]
    public class ExamBaseElement : Entity
    {
        public Guid AssessmentId { get; set; }

        public Guid LevelId { get; set; }

        public Guid QcaCodeId { get; set; }

        public string? QualAccrNumber { get; set; }

        public string? ElementCode { get; set; }

        // Subject discount code — stops double-counting in performance measures.
        [StringLength(10)]
        public string? DiscountCode { get; set; }

        public ExamAssessment? Assessment { get; set; }
        public SubjectCode? QcaCode { get; set; }
        public ExamQualificationLevel? Level { get; set; }
    }
}