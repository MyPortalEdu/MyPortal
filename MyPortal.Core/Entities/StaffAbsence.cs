using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("StaffAbsences")]
    public class StaffAbsence : Entity
    {
        public Guid StaffMemberId { get; set; }

        public Guid AbsenceTypeId { get; set; }

        public Guid? IllnessTypeId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsConfidential { get; set; }

        public string? Notes { get; set; }

        public StaffMember? StaffMember { get; set; }
        public StaffAbsenceType? AbsenceType { get; set; }
        public StaffIllnessType? IllnessType { get; set; }
    }
}