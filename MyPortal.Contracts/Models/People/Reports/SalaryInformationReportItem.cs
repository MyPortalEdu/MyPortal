namespace MyPortal.Contracts.Models.People.Reports;

/// <summary>
/// One row of the Salary Information report: a staff member's current open contract as at the
/// report's effective date, with pay-scale point and pension scheme resolved.
/// </summary>
public class SalaryInformationReportItem
{
    public string StaffCode { get; set; } = null!;
    public string StaffName { get; set; } = null!;
    public string? ServiceTerm { get; set; }
    public string? PostTitle { get; set; }
    public string? PayScale { get; set; }
    public string? PayPoint { get; set; }
    public decimal Fte { get; set; }

    /// <summary>Full-time-equivalent salary (the contract salary grossed up by FTE).</summary>
    public decimal? FullTimeSalary { get; set; }

    /// <summary>Actual salary payable (the stored contract salary, already FTE-adjusted).</summary>
    public decimal? ActualSalary { get; set; }

    public string? PensionScheme { get; set; }
    public DateTime? ContractStartDate { get; set; }
}
