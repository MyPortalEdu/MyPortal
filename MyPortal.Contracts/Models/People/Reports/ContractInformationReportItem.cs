namespace MyPortal.Contracts.Models.People.Reports;

/// <summary>
/// One row of the Contract Information report: an open contract for a staff member as at the
/// report's effective date, with its terms, role and pay point.
/// </summary>
public class ContractInformationReportItem
{
    public string StaffCode { get; set; } = null!;
    public string StaffName { get; set; } = null!;
    public string? ServiceTerm { get; set; }
    public string? PostTitle { get; set; }
    public string? Role { get; set; }
    public string? ContractType { get; set; }
    public decimal Fte { get; set; }
    public decimal? HoursPerWeek { get; set; }
    public decimal? WeeksPerYear { get; set; }
    public string? PayScale { get; set; }
    public string? PayPoint { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
