using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People.Reports;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;

namespace MyPortal.Data.Repositories;

/// <summary>
/// Backs the predefined HR reports. Entity type is nominal — every method is a raw read via
/// <see cref="SqlResourceLoader"/> and Dapper; nothing here mutates the StaffMember aggregate.
/// </summary>
public class StaffReportRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<StaffMember>(factory, authorizationService), IStaffReportRepository
{
    public Task<IReadOnlyList<SalaryInformationReportItem>> GetSalaryInformationAsync(string staffType,
        DateTime effectiveDate, CancellationToken cancellationToken) =>
        QueryAsync<SalaryInformationReportItem>("People.Reports.GetSalaryInformationReport.sql",
            new { staffType, effectiveDate }, cancellationToken);

    public Task<IReadOnlyList<ContractInformationReportItem>> GetContractInformationAsync(string staffType,
        DateTime effectiveDate, CancellationToken cancellationToken) =>
        QueryAsync<ContractInformationReportItem>("People.Reports.GetContractInformationReport.sql",
            new { staffType, effectiveDate }, cancellationToken);

    public Task<IReadOnlyList<ContractAnalysisReportItem>> GetContractAnalysisAsync(string staffType,
        DateTime effectiveDate, CancellationToken cancellationToken) =>
        QueryAsync<ContractAnalysisReportItem>("People.Reports.GetContractAnalysisReport.sql",
            new { staffType, effectiveDate }, cancellationToken);

    public Task<IReadOnlyList<TerminatingContractReportItem>> GetTerminatingContractsAsync(DateTime startDate,
        DateTime endDate, CancellationToken cancellationToken) =>
        QueryAsync<TerminatingContractReportItem>("People.Reports.GetTerminatingContractsReport.sql",
            new { startDate, endDate }, cancellationToken);

    public Task<IReadOnlyList<IndividualAbsenceReportItem>> GetIndividualAbsenceAsync(Guid staffMemberId,
        Guid? absenceTypeId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken) =>
        QueryAsync<IndividualAbsenceReportItem>("People.Reports.GetIndividualAbsenceReport.sql",
            new { staffMemberId, absenceTypeId, startDate, endDate }, cancellationToken);

    public Task<IReadOnlyList<StaffAbsenceAnalysisReportItem>> GetStaffAbsenceAnalysisAsync(Guid? absenceTypeId,
        DateTime startDate, DateTime endDate, CancellationToken cancellationToken) =>
        QueryAsync<StaffAbsenceAnalysisReportItem>("People.Reports.GetStaffAbsenceAnalysisReport.sql",
            new { absenceTypeId, startDate, endDate }, cancellationToken);

    public Task<IReadOnlyList<LongTermAbsenceReportItem>> GetLongTermAbsenceAsync(DateTime startDate,
        DateTime endDate, decimal minDays, CancellationToken cancellationToken) =>
        QueryAsync<LongTermAbsenceReportItem>("People.Reports.GetLongTermAbsenceReport.sql",
            new { startDate, endDate, minDays }, cancellationToken);

    public Task<IReadOnlyList<StaffTrainingReportItem>> GetStaffTrainingAsync(Guid? staffMemberId,
        DateTime startDate, DateTime endDate, CancellationToken cancellationToken) =>
        QueryAsync<StaffTrainingReportItem>("People.Reports.GetStaffTrainingReport.sql",
            new { staffMemberId, startDate, endDate }, cancellationToken);

    public Task<IReadOnlyList<TrainingCourseAttendeeReportItem>> GetTrainingCourseAttendeesAsync(
        Guid trainingCourseId, CancellationToken cancellationToken) =>
        QueryAsync<TrainingCourseAttendeeReportItem>("People.Reports.GetTrainingCourseReport.sql",
            new { trainingCourseId }, cancellationToken);

    public Task<IReadOnlyList<TrainingCourseOption>> GetTrainingCourseOptionsAsync(
        CancellationToken cancellationToken) =>
        QueryAsync<TrainingCourseOption>("People.Reports.GetTrainingCourseOptions.sql", new { },
            cancellationToken);

    public Task<IReadOnlyList<AbsenceTypeOption>> GetAbsenceTypeOptionsAsync(
        CancellationToken cancellationToken) =>
        QueryAsync<AbsenceTypeOption>("People.Reports.GetAbsenceTypeOptions.sql", new { },
            cancellationToken);

    private async Task<IReadOnlyList<T>> QueryAsync<T>(string resource, object parameters,
        CancellationToken cancellationToken)
    {
        var sql = SqlResourceLoader.Load(resource);

        var (conn, owns) = AcquireConnection(null);

        try
        {
            var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);
            var rows = await conn.QueryAsync<T>(command);
            return rows.AsList();
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }
}
