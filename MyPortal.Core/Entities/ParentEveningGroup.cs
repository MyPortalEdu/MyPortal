using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ParentEveningGroups")]
    public class ParentEveningGroup : Entity
    {
        public Guid ParentEveningId { get; set; }

        public Guid StudentGroupId { get; set; }

        public ParentEvening? ParentEvening { get; set; }
        public StudentGroup? StudentGroup { get; set; }
    }
}