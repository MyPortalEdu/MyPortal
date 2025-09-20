using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamRoomSeatBlocks")]
    public class ExamRoomSeatBlock : Entity
    {
        public Guid ExamRoomId { get; set; }

        public int SeatRow { get; set; }

        public int SeatColumn { get; set; }

        public string? Comments { get; set; }

        public ExamRoom? ExamRoom { get; set; }
    }
}