using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("MarksheetTemplates")]
    public class MarksheetTemplate : Entity
    {
        [Required]
        [StringLength(128)]
        public required string Name { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; }
    }
}