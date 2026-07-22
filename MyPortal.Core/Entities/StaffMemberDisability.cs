using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("StaffMemberDisabilities")]
    public class StaffMemberDisability : Entity
    {
        public Guid StaffMemberId { get; set; }

        public Guid DisabilityId { get; set; }

        public DateTime? DateAdvised { get; set; }

        // Equality Act "long-term" limb: 12 months or more.
        public bool IsLongTerm { get; set; }

        public bool AffectsWorkingAbility { get; set; }

        [StringLength(512)]
        public string? AssistanceRequired { get; set; }

        public StaffMember? StaffMember { get; set; }
        public Disability? Disability { get; set; }
    }
}
