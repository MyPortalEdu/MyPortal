using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    // Join row linking a staff member to a declared disability. Staff-scoped (the
    // Equality area is staff-only, no Managed scope) and kept alongside the
    // free-text DisabilityDetails / HasDisability flag on StaffMember.
    [Table("StaffMemberDisabilities")]
    public class StaffMemberDisability : Entity
    {
        public Guid StaffMemberId { get; set; }

        public Guid DisabilityId { get; set; }

        public StaffMember? StaffMember { get; set; }
        public Disability? Disability { get; set; }
    }
}
