using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("StudentGroups")]
    public class StudentGroup : LookupEntity, ISystemEntity
    {
        [Required] [StringLength(10)] 
        public string Code { get; set; } = null!;

        public Guid? PromoteToGroupId { get; set; }

        public Guid? MainSupervisorId { get; set; }

        public int? MaxMembers { get; set; }
        
        [StringLength(256)]
        public string? Notes { get; set; }

        public bool IsSystem { get; set; }

        public StudentGroup? PromoteToGroup { get; set; }
        public StudentGroupSupervisor? MainSupervisor { get; set; }
    }
}