namespace MyPortal.Contracts.Models.People.Reports;

/// <summary>
/// One row of the Contract Analysis report: a service term with open contracts as at the effective
/// date, summarised — contract and staff counts, teaching/support split, and total FTE.
/// </summary>
public class ContractAnalysisReportItem
{
    public string ServiceTerm { get; set; } = null!;
    public int ContractCount { get; set; }
    public int StaffCount { get; set; }
    public int TeachingCount { get; set; }
    public int SupportCount { get; set; }
    public decimal TotalFte { get; set; }
}
