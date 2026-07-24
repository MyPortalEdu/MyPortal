namespace MyPortal.Contracts.Models.People;

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
