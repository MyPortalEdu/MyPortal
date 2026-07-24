using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Contracts.Models.People.Reports;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.People;
using MyPortal.Services.Interfaces.Providers;

namespace MyPortal.Services.People;

public class StaffReportService(
    IAuthorizationService authorizationService,
    ILogger<StaffReportService> logger,
    IStaffReportRepository staffReportRepository,
    IDateTimeProvider dateTimeProvider)
    : BaseService(authorizationService, logger), IStaffReportService
{
    public async Task<IReadOnlyList<SalaryInformationReportItem>> GetSalaryInformationAsync(string? staffType,
        DateTime? effectiveDate, CancellationToken cancellationToken)
    {
        // Salary is employment-sensitive, so the report is gated on the All-scope employment permission.
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffEmploymentDetails,
            cancellationToken);

        var type = NormaliseStaffType(staffType);
        var effective = (effectiveDate ?? dateTimeProvider.UtcNow.Date).Date;

        return await staffReportRepository.GetSalaryInformationAsync(type, effective, cancellationToken);
    }

    public async Task<IReadOnlyList<ContractInformationReportItem>> GetContractInformationAsync(
        string? staffType, DateTime? effectiveDate, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffEmploymentDetails,
            cancellationToken);

        var type = NormaliseStaffType(staffType);
        var effective = (effectiveDate ?? dateTimeProvider.UtcNow.Date).Date;

        return await staffReportRepository.GetContractInformationAsync(type, effective, cancellationToken);
    }

    public async Task<IReadOnlyList<ContractAnalysisReportItem>> GetContractAnalysisAsync(
        string? staffType, DateTime? effectiveDate, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffEmploymentDetails,
            cancellationToken);

        var type = NormaliseStaffType(staffType);
        var effective = (effectiveDate ?? dateTimeProvider.UtcNow.Date).Date;

        return await staffReportRepository.GetContractAnalysisAsync(type, effective, cancellationToken);
    }

    public async Task<IReadOnlyList<TerminatingContractReportItem>> GetTerminatingContractsAsync(
        DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffEmploymentDetails,
            cancellationToken);

        var start = (startDate ?? dateTimeProvider.UtcNow.Date).Date;
        var end = (endDate ?? start.AddMonths(3)).Date;

        return await staffReportRepository.GetTerminatingContractsAsync(start, end, cancellationToken);
    }

    public async Task<IReadOnlyList<IndividualAbsenceReportItem>> GetIndividualAbsenceAsync(Guid staffMemberId,
        Guid? absenceTypeId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffAbsences,
            cancellationToken);

        var (start, end) = ResolvePeriod(startDate, endDate);
        return await staffReportRepository.GetIndividualAbsenceAsync(staffMemberId, absenceTypeId, start, end,
            cancellationToken);
    }

    public async Task<IReadOnlyList<StaffAbsenceAnalysisReportItem>> GetStaffAbsenceAnalysisAsync(
        Guid? absenceTypeId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffAbsences,
            cancellationToken);

        var (start, end) = ResolvePeriod(startDate, endDate);
        return await staffReportRepository.GetStaffAbsenceAnalysisAsync(absenceTypeId, start, end,
            cancellationToken);
    }

    public async Task<IReadOnlyList<LongTermAbsenceReportItem>> GetLongTermAbsenceAsync(DateTime? startDate,
        DateTime? endDate, decimal? minDays, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffAbsences,
            cancellationToken);

        var (start, end) = ResolvePeriod(startDate, endDate);
        var threshold = minDays is > 0 ? minDays.Value : 20m;
        return await staffReportRepository.GetLongTermAbsenceAsync(start, end, threshold, cancellationToken);
    }

    public async Task<IReadOnlyList<StaffTrainingReportItem>> GetStaffTrainingAsync(Guid? staffMemberId,
        DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffEmploymentDetails,
            cancellationToken);

        var (start, end) = ResolvePeriod(startDate, endDate);
        return await staffReportRepository.GetStaffTrainingAsync(staffMemberId, start, end, cancellationToken);
    }

    public async Task<IReadOnlyList<TrainingCourseAttendeeReportItem>> GetTrainingCourseAttendeesAsync(
        Guid trainingCourseId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffEmploymentDetails,
            cancellationToken);

        return await staffReportRepository.GetTrainingCourseAttendeesAsync(trainingCourseId, cancellationToken);
    }

    public async Task<IReadOnlyList<TrainingCourseOption>> GetTrainingCourseOptionsAsync(
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffEmploymentDetails,
            cancellationToken);

        return await staffReportRepository.GetTrainingCourseOptionsAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AbsenceTypeOption>> GetAbsenceTypeOptionsAsync(
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffAbsences,
            cancellationToken);

        return await staffReportRepository.GetAbsenceTypeOptionsAsync(cancellationToken);
    }

    // Absence/training periods default to the last twelve months up to today.
    private (DateTime Start, DateTime End) ResolvePeriod(DateTime? startDate, DateTime? endDate)
    {
        var end = (endDate ?? dateTimeProvider.UtcNow.Date).Date;
        var start = (startDate ?? end.AddYears(-1)).Date;
        return (start, end);
    }

    private static string NormaliseStaffType(string? staffType) => staffType?.Trim() switch
    {
        "Teaching" => "Teaching",
        "Support" => "Support",
        _ => "All"
    };
}
