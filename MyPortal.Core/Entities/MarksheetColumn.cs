using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("MarksheetColumns")]
    public class MarksheetColumn : Entity
    {
        public Guid TemplateId { get; set; }

        public Guid AspectId { get; set; }

        public Guid ResultSetId { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsReadOnly { get; set; }

        public MarksheetTemplate? Template { get; set; }
        public Aspect? Aspect { get; set; }
        public ResultSet? ResultSet { get; set; }
    }
}