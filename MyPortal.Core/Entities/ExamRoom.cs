using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamRooms")]
    public class ExamRoom : Entity
    {
        public Guid RoomId { get; set; }

        public int Columns { get; set; }

        public int Rows { get; set; }

        public Room? Room { get; set; }
    }
}