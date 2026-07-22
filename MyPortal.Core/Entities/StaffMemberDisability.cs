using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    // A disability a staff member has declared, with the detail the Equality Act duty needs:
    // when it was advised, whether it's long-term, whether it affects their working ability, and
    // the reasonable adjustments agreed. Keyed to the CBDS-coded Disability lookup rather than
    // free text, so it stays usable for statutory returns.
    [Table("StaffMemberDisabilities")]
    public class StaffMemberDisability : Entity
    {
        public Guid StaffMemberId { get; set; }

        public Guid DisabilityId { get; set; }

        // When the staff member advised the school.
        public DateTime? DateAdvised { get; set; }

        // Equality Act "long-term" limb — lasted or expected to last 12 months or more.
        public bool IsLongTerm { get; set; }

        public bool AffectsWorkingAbility { get; set; }

        // Reasonable adjustments agreed / assistance required.
        [StringLength(512)]
        public string? AssistanceRequired { get; set; }

        public StaffMember? StaffMember { get; set; }
        public Disability? Disability { get; set; }
    }
}
