using MyPortal.Contracts.Models.People.Reports;

namespace MyPortal.Services.Interfaces.People;

public interface IStaffReportService
{
    /// <summary>
    /// Salary Information report: each staff member's current open contract as at
    /// <paramref name="effectiveDate"/> (defaults to today). <paramref name="staffType"/> is
    /// 'All' | 'Teaching' | 'Support'. Gated on the All-scope employment permission.
    /// </summary>
    Task<IReadOnlyList<SalaryInformationReportItem>> GetSalaryInformationAsync(string? staffType,
        DateTime? effectiveDate, CancellationToken cancellationToken);

    /// <summary>Contract Information: open contract details per staff member as at the effective date.</summary>
    Task<IReadOnlyList<ContractInformationReportItem>> GetContractInformationAsync(string? staffType,
        DateTime? effectiveDate, CancellationToken cancellationToken);

    /// <summary>Contract Analysis: open contracts summarised by service term as at the effective date.</summary>
    Task<IReadOnlyList<ContractAnalysisReportItem>> GetContractAnalysisAsync(string? staffType,
        DateTime? effectiveDate, CancellationToken cancellationToken);

    /// <summary>
    /// Terminating Contracts: contracts ending within the period. <paramref name="startDate"/> defaults
    /// to today and <paramref name="endDate"/> to three months after the start.
    /// </summary>
    Task<IReadOnlyList<TerminatingContractReportItem>> GetTerminatingContractsAsync(DateTime? startDate,
        DateTime? endDate, CancellationToken cancellationToken);

    /// <summary>Individual Absence report for one staff member. Gated on the All-scope absence permission.</summary>
    Task<IReadOnlyList<IndividualAbsenceReportItem>> GetIndividualAbsenceAsync(Guid staffMemberId,
        Guid? absenceTypeId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken);

    /// <summary>Staff Absence Analysis report, summarised by service term.</summary>
    Task<IReadOnlyList<StaffAbsenceAnalysisReportItem>> GetStaffAbsenceAnalysisAsync(Guid? absenceTypeId,
        DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken);

    /// <summary>Long-Term Absence Analysis report. <paramref name="minDays"/> defaults to 20 working days.</summary>
    Task<IReadOnlyList<LongTermAbsenceReportItem>> GetLongTermAbsenceAsync(DateTime? startDate,
        DateTime? endDate, decimal? minDays, CancellationToken cancellationToken);

    /// <summary>Staff Training report, optionally for one staff member.</summary>
    Task<IReadOnlyList<StaffTrainingReportItem>> GetStaffTrainingAsync(Guid? staffMemberId,
        DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken);

    /// <summary>Training Course report: attendees for one course.</summary>
    Task<IReadOnlyList<TrainingCourseAttendeeReportItem>> GetTrainingCourseAttendeesAsync(
        Guid trainingCourseId, CancellationToken cancellationToken);

    /// <summary>Active training courses for the Training Course report's picker.</summary>
    Task<IReadOnlyList<TrainingCourseOption>> GetTrainingCourseOptionsAsync(CancellationToken cancellationToken);

    /// <summary>Absence types for the absence reports' type filter.</summary>
    Task<IReadOnlyList<AbsenceTypeOption>> GetAbsenceTypeOptionsAsync(CancellationToken cancellationToken);
}
