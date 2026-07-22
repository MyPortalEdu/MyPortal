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

        // ---- Statutory / payroll treatment (HR-only: set by All-scope editors) ----

        // The authorised rate this absence is paid at (SSP, occupational full/half pay, unpaid).
        public Guid? AuthorisedPayRateId { get; set; }

        // The category reported to the payroll provider.
        public Guid? PayrollReasonId { get; set; }

        // "SSP exclusion advised" — the staff member is not entitled to statutory sick pay.
        public bool SspExcluded { get; set; }

        // Recorded separately from the date range: a range can span non-working days, and a
        // part-day absence has no whole-day equivalent.
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