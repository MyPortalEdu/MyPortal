using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("RegGroups")]
    public class RegGroup : Entity, IStudentGroupEntity
    {
        public Guid StudentGroupId { get; set; }

        public Guid YearGroupId { get; set; }

        public Guid? RoomId { get; set; }

        public StudentGroup? StudentGroup { get; set; }

        public YearGroup? YearGroup { get; set; }
    }
}