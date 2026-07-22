namespace MyPortal.Contracts.Models.People;

/// <summary>An absence row in the upsert. Null id is a new row; populated id updates;
/// omitted rows are deleted server-side (scoped so a line manager never affects
/// confidential rows). IsConfidential is honoured only for All-scope (HR) editors.</summary>
public class StaffAbsenceUpsertItem
{
    public Guid? Id { get; set; }
    public Guid AbsenceTypeId { get; set; }
    public Guid? IllnessTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsConfidential { get; set; }
    public string? Notes { get; set; }

    // Payroll treatment — ignored unless the editor holds All scope (HR), like IsConfidential.
    public Guid? AuthorisedPayRateId { get; set; }
    public Guid? PayrollReasonId { get; set; }
    public bool SspExcluded { get; set; }
    public decimal? WorkingDaysLost { get; set; }
    public decimal? HoursLost { get; set; }
    public bool IsIndustrialInjury { get; set; }

    public List<StaffAbsenceCertificateUpsertItem> Certificates { get; set; } = [];
}
