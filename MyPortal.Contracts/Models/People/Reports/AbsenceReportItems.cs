namespace MyPortal.Contracts.Models.People.Reports;

/// <summary>A selectable absence type for the absence reports' type filter.</summary>
public class AbsenceTypeOption
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

/// <summary>One absence for the Individual Absence report (a single staff member over a period).</summary>
public class IndividualAbsenceReportItem
{
    public string AbsenceType { get; set; } = null!;
    public string? IllnessType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? WorkingDaysLost { get; set; }
    public decimal? HoursLost { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// One row of the Staff Absence Analysis report: absences over a period summarised by service term.
/// </summary>
public class StaffAbsenceAnalysisReportItem
{
    public string ServiceTerm { get; set; } = null!;
    public int AbsenceCount { get; set; }
    public int StaffCount { get; set; }
    public decimal TotalWorkingDaysLost { get; set; }
}

/// <summary>
/// One extended absence for the Long-Term Absence Analysis report (sickness / maternity / pregnancy
/// absences over a threshold of working days lost within the period). A pragmatic slice of SIMS'
/// full three-year, FTE-weighted insurance report.
/// </summary>
public class LongTermAbsenceReportItem
{
    public string StaffCode { get; set; } = null!;
    public string StaffName { get; set; } = null!;
    public string AbsenceType { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? WorkingDaysLost { get; set; }
}
