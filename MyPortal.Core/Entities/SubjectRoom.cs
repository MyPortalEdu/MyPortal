using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SubjectRooms")]
    public class SubjectRoom : Entity
    {
        public Guid SubjectId { get; set; }

        public Guid RoomId { get; set; }

        public Subject? Subject { get; set; }
        public Room? Room { get; set; }
    }
}
