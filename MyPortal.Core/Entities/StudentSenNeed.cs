using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("StudentSenNeeds")]
    public class StudentSenNeed : Entity
    {
        public Guid StudentId { get; set; }

        public Guid SenTypeId { get; set; }

        [StringLength(1024)]
        public string? Description { get; set; }

        public DateTime StartDate { get; set; }

        // Null while the need is current; set when it resolves (kept on record as history).
        public DateTime? EndDate { get; set; }

        // Priority ranking: 1 = primary need, 2+ = secondary needs in order.
        public int Rank { get; set; }

        public Student? Student { get; set; }
        public SenType? SenType { get; set; }
    }
}
