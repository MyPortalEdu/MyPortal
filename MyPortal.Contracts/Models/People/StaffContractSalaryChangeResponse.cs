namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A recorded movement of a contract's pay point / salary. Read-only — the server writes these
/// automatically when an update changes the point or the annual salary.
/// </summary>
public class StaffContractSalaryChangeResponse
{
    public Guid Id { get; set; }
    public Guid? OldPayScalePointId { get; set; }
    public Guid? NewPayScalePointId { get; set; }
    public decimal? OldAnnualSalary { get; set; }
    public decimal? NewAnnualSalary { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
}
