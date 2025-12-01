using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ParentEvenings")]
    public class ParentEvening : Entity
    {
        public Guid EventId { get; set; }

        [Required] 
        [StringLength(128)] 
        public string Name { get; set; } = null!;

        public DateTime BookingOpened { get; set; }

        public DateTime BookingClosed { get; set; }

        public DiaryEvent? Event { get; set; }
    }
}