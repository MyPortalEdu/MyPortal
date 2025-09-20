using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Marksheets")]
    public class Marksheet : Entity
    {
        public Guid MarksheetTemplateId { get; set; }

        public Guid StudentGroupId { get; set; }

        public bool IsCompleted { get; set; }

        public MarksheetTemplate? Template { get; set; }
        public StudentGroup? StudentGroup { get; set; }
    }
}