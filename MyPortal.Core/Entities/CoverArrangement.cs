using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("CoverArrangements")]
    public class CoverArrangement : Entity
    {
        public Guid WeekId { get; set; }

        public Guid SessionId { get; set; }

        public Guid? TeacherId { get; set; }

        public Guid? RoomId { get; set; }

        public string? Comments { get; set; }

        public AttendanceWeek? Week { get; set; }
        public Session? Session { get; set; }
        public StaffMember? Teacher { get; set; }
        public Room? Room { get; set; }
    }
}