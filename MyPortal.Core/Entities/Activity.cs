using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Activities")]
    public class Activity : Entity, IStudentGroupEntity
    {
        public Guid StudentGroupId { get; set; }

        public StudentGroup? StudentGroup { get; set; }
    }
}