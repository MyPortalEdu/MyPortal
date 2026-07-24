using MyPortal.Contracts.Models.People.Reports;

namespace MyPortal.Data.Interfaces;

/// <summary>
/// Read-only queries backing the predefined HR (Personnel) reports. Each method maps one report's
/// SQL to its row DTO; no entity mutation.
/// </summary>
public interface IStaffReportRepository
{
    /// <summary>
    /// Salary Information: current open contract per staff member as at <paramref name="effectiveDate"/>.
    /// </summary>
    Task<IReadOnlyList<SalaryInformationReportItem>> GetSalaryInformationAsync(string staffType,
        DateTime effectiveDate, CancellationToken cancellationToken);

    /// <summary>Contract Information: open contract details per staff member as at the effective date.</summary>
    Task<IReadOnlyList<ContractInformationReportItem>> GetContractInformationAsync(string staffType,
        DateTime effectiveDate, CancellationToken cancellationToken);

    /// <summary>Contract Analysis: open contracts summarised by service term as at the effective date.</summary>
    Task<IReadOnlyList<ContractAnalysisReportItem>> GetContractAnalysisAsync(string staffType,
        DateTime effectiveDate, CancellationToken cancellationToken);

    /// <summary>Terminating Contracts: contracts ending within [startDate, endDate].</summary>
    Task<IReadOnlyList<TerminatingContractReportItem>> GetTerminatingContractsAsync(DateTime startDate,
        DateTime endDate, CancellationToken cancellationToken);

    /// <summary>Individual Absence: absences for one staff member over a period, optionally of one type.</summary>
    Task<IReadOnlyList<IndividualAbsenceReportItem>> GetIndividualAbsenceAsync(Guid staffMemberId,
        Guid? absenceTypeId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);

    /// <summary>Staff Absence Analysis: absences over a period summarised by service term.</summary>
    Task<IReadOnlyList<StaffAbsenceAnalysisReportItem>> GetStaffAbsenceAnalysisAsync(Guid? absenceTypeId,
        DateTime startDate, DateTime endDate, CancellationToken cancellationToken);

    /// <summary>Long-Term Absence: extended sickness/maternity absences over the period.</summary>
    Task<IReadOnlyList<LongTermAbsenceReportItem>> GetLongTermAbsenceAsync(DateTime startDate,
        DateTime endDate, decimal minDays, CancellationToken cancellationToken);

    /// <summary>Staff Training: training records, optionally for one staff member, within the period.</summary>
    Task<IReadOnlyList<StaffTrainingReportItem>> GetStaffTrainingAsync(Guid? staffMemberId,
        DateTime startDate, DateTime endDate, CancellationToken cancellationToken);

    /// <summary>Training Course: attendees for one course.</summary>
    Task<IReadOnlyList<TrainingCourseAttendeeReportItem>> GetTrainingCourseAttendeesAsync(
        Guid trainingCourseId, CancellationToken cancellationToken);

    /// <summary>Active training courses for the Training Course report's picker.</summary>
    Task<IReadOnlyList<TrainingCourseOption>> GetTrainingCourseOptionsAsync(CancellationToken cancellationToken);

    /// <summary>Absence types for the absence reports' type filter.</summary>
    Task<IReadOnlyList<AbsenceTypeOption>> GetAbsenceTypeOptionsAsync(CancellationToken cancellationToken);
}
