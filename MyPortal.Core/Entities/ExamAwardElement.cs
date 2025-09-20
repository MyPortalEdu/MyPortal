using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamAwardElements")]
    public class ExamAwardElement : Entity
    {
        public Guid AwardId { get; set; }

        public Guid ElementId { get; set; }

        public ExamAward? Award { get; set; }
        public ExamElement? Element { get; set; }
    }
}