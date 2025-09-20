using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("CurriculumGroups")]
    public class CurriculumGroup : Entity
    {
        public Guid BlockId { get; set; }

        public Guid StudentGroupId { get; set; }

        public CurriculumBlock? Block { get; set; }
        public StudentGroup? StudentGroup { get; set; }
    }
}