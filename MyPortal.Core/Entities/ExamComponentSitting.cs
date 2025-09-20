using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamComponentSittings")]
    public class ExamComponentSitting : Entity
    {
        public Guid ComponentId { get; set; }

        public Guid ExamRoomId { get; set; }

        public TimeSpan? ActualStartTime { get; set; }

        public int ExtraTimePercent { get; set; }

        public virtual ExamComponent? Component { get; set; }
        public virtual ExamRoom? Room { get; set; }
    }
}