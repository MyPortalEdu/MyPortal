using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Database.BaseTypes;

namespace MyPortal.Database.Models.Entity
{
    [Table("subject_staff_member_role")]
    public class SubjectStaffMemberRole : LookupItem
    {
        public SubjectStaffMemberRole()
        {
            StaffMembers = new HashSet<SubjectStaffMember>();
        }

        public bool SubjectLeader { get; set; }


        public virtual ICollection<SubjectStaffMember> StaffMembers { get; set; }
    }
}