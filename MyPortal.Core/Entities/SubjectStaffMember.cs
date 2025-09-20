using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SubjectStaffMembers")]
    public class SubjectStaffMember : Entity
    {
        public Guid SubjectId { get; set; }

        public Guid StaffMemberId { get; set; }

        public Guid RoleId { get; set; }

        public Subject? Subject { get; set; }
        public StaffMember? StaffMember { get; set; }
        public SubjectStaffMemberRole? Role { get; set; }
    }
}