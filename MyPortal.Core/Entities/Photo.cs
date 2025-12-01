using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Photos")]
    public class Photo : AuditableEntity
    {
        public Guid DocumentId { get; set; }

        public DateTime? PhotoDate { get; set; }

        public Document? Document { get; set; }
    }
}