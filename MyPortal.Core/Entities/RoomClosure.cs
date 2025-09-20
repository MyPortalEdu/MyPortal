using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("RoomClosures")]
    public class RoomClosure : Entity
    {
        public Guid RoomId { get; set; }

        public Guid ReasonId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
        
        [StringLength(256)]
        public string? Notes { get; set; }

        public Room? Room { get; set; }
        public RoomClosureReason? Reason { get; set; }
    }
}