using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Sessions")]
    public class Session : Entity
    {
        public Guid ClassId { get; set; }

        public Guid TeacherId { get; set; }

        public Guid? RoomId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public StaffMember? Teacher { get; set; }
        public Class? Class { get; set; }
        public Room? Room { get; set; }
    }
}