using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SenStatusHistories")]
    public class SenStatusHistory : Entity
    {
        public Guid StudentId { get; set; }

        public Guid SenStatusId { get; set; }

        public DateTime StartDate { get; set; }

        // Null while this is the current status; set when a later status supersedes it.
        public DateTime? EndDate { get; set; }

        public Student? Student { get; set; }
        public SenStatus? SenStatus { get; set; }
    }
}
