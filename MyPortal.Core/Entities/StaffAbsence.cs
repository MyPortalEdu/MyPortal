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

        public Guid? AuthorisedPayRateId { get; set; }

        public Guid? PayrollReasonId { get; set; }

        public bool SspExcluded { get; set; }

        // Not derivable from the date range: it can span non-working days, and part-days have no
        // whole-day equivalent.
        [Column(TypeName = "decimal(6,2)")]
        public decimal? WorkingDaysLost { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? HoursLost { get; set; }

        // Orthogonal to the absence type — an illness can also be an industrial injury.
        public bool IsIndustrialInjury { get; set; }

        public StaffMember? StaffMember { get; set; }
        public StaffAbsenceType? AbsenceType { get; set; }
        public StaffIllnessType? IllnessType { get; set; }
        public StaffAbsencePayRate? AuthorisedPayRate { get; set; }
        public StaffAbsencePayrollReason? PayrollReason { get; set; }
    }
}