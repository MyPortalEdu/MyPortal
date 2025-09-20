using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ParentEveningStaffMembers")]
    public class ParentEveningStaffMember : Entity
    {
        public Guid ParentEveningId { get; set; }

        public Guid StaffMemberId { get; set; }

        public DateTime? AvailableFrom { get; set; }

        public DateTime? AvailableTo { get; set; }

        public int AppointmentLength { get; set; }

        public int BreakLimit { get; set; }

        public ParentEvening? ParentEvening { get; set; }
        public StaffMember? StaffMember { get; set; }
    }
}