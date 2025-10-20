using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Photos")]
    public class Photo : AuditableEntity
    {
        public Guid FileId { get; set; }

        public DateTime PhotoDate { get; set; }

        [Required]
        public required string MimeType { get; set; }
    }
}