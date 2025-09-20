using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Houses")]
    public class House : Entity, IStudentGroupEntity
    {
        public Guid StudentGroupId { get; set; }

        [StringLength(10)] 
        public string? ColourCode { get; set; }

        public StudentGroup? StudentGroup { get; set; }
    }
}