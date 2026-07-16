namespace MyPortal.Contracts.Models.People;

/// <summary>
/// One contract in an employment's replace payload. A null <see cref="Id"/> is a new row; a
/// populated <see cref="Id"/> updates the existing one. Contracts present in the database but
/// absent from the payload are soft-deleted.
/// </summary>
public class StaffContractUpsertItem
{
    public Guid? Id { get; set; }
    public Guid ContractTypeId { get; set; }
    public Guid? StaffRoleId { get; set; }
    public Guid? ServiceTermId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? PayScaleId { get; set; }
    public Guid? PayScalePointId { get; set; }
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
}
