namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A single contract within an employment spell. Lookup ids resolve against the option lists on
/// <see cref="StaffEmploymentDetailsResponse"/>.
/// </summary>
public class StaffContractResponse
{
    public Guid Id { get; set; }
    public Guid ContractTypeId { get; set; }
    public Guid? StaffRoleId { get; set; }
    public Guid? ServiceTermId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? PayScaleId { get; set; }
    public Guid? PayScalePointId { get; set; }
    public Guid? PostId { get; set; }
    public Guid? SuperannuationSchemeId { get; set; }
    public bool NiContractedOut { get; set; }
    public string PostTitle { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Fte { get; set; }
    public decimal? HoursPerWeek { get; set; }
    public decimal? WeeksPerYear { get; set; }
    public decimal? AnnualSalary { get; set; }
    public bool IsAgencySupply { get; set; }
    public bool SafeguardedSalary { get; set; }
    public bool DailyRate { get; set; }

    public List<StaffContractAllowanceResponse> Allowances { get; set; } = [];

    public List<StaffContractSuspensionResponse> Suspensions { get; set; } = [];

    public List<StaffContractSalaryChangeResponse> SalaryChanges { get; set; } = [];
}
