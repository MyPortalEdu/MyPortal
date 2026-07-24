namespace MyPortal.Contracts.Models.People.Reports;

/// <summary>
/// One row of the Terminating Contracts report: a contract whose end date falls within the chosen
/// reporting period.
/// </summary>
public class TerminatingContractReportItem
{
    public string StaffCode { get; set; } = null!;
    public string StaffName { get; set; } = null!;
    public string? PostTitle { get; set; }
    public string? ContractType { get; set; }
    public string? ServiceTerm { get; set; }
    public decimal Fte { get; set; }
    public DateTime EndDate { get; set; }
}
