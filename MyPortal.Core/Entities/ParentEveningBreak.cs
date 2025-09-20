using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ParentEveningBreaks")]
    public class ParentEveningBreak : Entity
    {
        public Guid ParentEveningStaffMemberId { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }
    }
}