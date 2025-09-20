using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamDates")]
    public class ExamDate : Entity
    {
        public Guid SessionId { get; set; }

        public int Duration { get; set; }

        public DateTime SittingDate { get; set; }

        public ExamSession? Session { get; set; }
    }
}