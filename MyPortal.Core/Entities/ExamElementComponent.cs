using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamElementComponents")]
    public class ExamElementComponent : Entity
    {
        public Guid ElementId { get; set; }

        public Guid ComponentId { get; set; }

        public ExamElement? Element { get; set; }
        public ExamComponent? Component { get; set; }
    }
}