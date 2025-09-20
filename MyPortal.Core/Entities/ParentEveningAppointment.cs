using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ParentEveningAppointments")]
    public class ParentEveningAppointment : Entity
    {
        public Guid ParentEveningStaffMemberId { get; set; }

        public Guid StudentId { get; set; }

        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public bool HasTeacherAccepted { get; set; }

        public bool HasParentAccepted { get; set; }

        public bool? HasAttended { get; set; }

        public ParentEveningStaffMember? ParentEveningStaffMember { get; set; }
        public Student? Student { get; set; }
    }
}