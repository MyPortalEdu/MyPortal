using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SubjectStaffMemberRoles")]
    public class SubjectStaffMemberRole : LookupEntity
    {
        public bool IsSubjectLeader { get; set; }
    }
}