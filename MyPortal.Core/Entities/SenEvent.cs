using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SenEvents")]
    public class SenEvent : Entity
    {
        public Guid StudentId { get; set; }

        public Guid SenEventTypeId { get; set; }

        public DateTime Date { get; set; }

        [Required] 
        public required string Note { get; set; }

        public Student? Student { get; set; }
        public SenEventType? SenEventType { get; set; }
    }
}