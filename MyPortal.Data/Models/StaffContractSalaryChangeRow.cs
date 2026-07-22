namespace MyPortal.Data.Models;

/// <summary>
/// A salary-change row with the changing user's display name already resolved, so the service
/// doesn't have to round-trip per row.
/// </summary>
public class StaffContractSalaryChangeRow
{
    public Guid Id { get; set; }
    public Guid StaffContractId { get; set; }
    public Guid? OldPayScalePointId { get; set; }
    public Guid? NewPayScalePointId { get; set; }
    public decimal? OldAnnualSalary { get; set; }
    public decimal? NewAnnualSalary { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
}
