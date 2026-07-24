using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.People.Reports;
using MyPortal.Services.Interfaces.People;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Predefined HR (Personnel) reports. Each endpoint runs one report from its criteria and returns
/// a flat, exportable row set. HR-only; individual reports gate on their own domain permission.
/// </summary>
public sealed class StaffReportsController(
    ProblemDetailsFactory problemFactory,
    ILogger<StaffReportsController> logger,
    IStaffReportService staffReportService)
    : BaseApiController(problemFactory, logger)
{
    /// <summary>Salary Information: current open contract per staff member as at the effective date.</summary>
    /// <param name="staffType">All | Teaching | Support. Defaults to All.</param>
    /// <param name="effectiveDate">Date the contract must be open on. Defaults to today.</param>
    [HttpGet("salary-information")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<SalaryInformationReportItem>), 200)]
    public async Task<IActionResult> GetSalaryInformationAsync([FromQuery] string? staffType = null,
        [FromQuery] DateTime? effectiveDate = null)
    {
        var result = await staffReportService.GetSalaryInformationAsync(staffType, effectiveDate,
            CancellationToken);
        return Ok(result);
    }

    /// <summary>Contract Information: open contract details per staff member as at the effective date.</summary>
    [HttpGet("contract-information")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<ContractInformationReportItem>), 200)]
    public async Task<IActionResult> GetContractInformationAsync([FromQuery] string? staffType = null,
        [FromQuery] DateTime? effectiveDate = null)
    {
        var result = await staffReportService.GetContractInformationAsync(staffType, effectiveDate,
            CancellationToken);
        return Ok(result);
    }

    /// <summary>Contract Analysis: open contracts summarised by service term as at the effective date.</summary>
    [HttpGet("contract-analysis")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<ContractAnalysisReportItem>), 200)]
    public async Task<IActionResult> GetContractAnalysisAsync([FromQuery] string? staffType = null,
        [FromQuery] DateTime? effectiveDate = null)
    {
        var result = await staffReportService.GetContractAnalysisAsync(staffType, effectiveDate,
            CancellationToken);
        return Ok(result);
    }

    /// <summary>Terminating Contracts: contracts ending within [startDate, endDate].</summary>
    /// <param name="startDate">Period start. Defaults to today.</param>
    /// <param name="endDate">Period end. Defaults to three months after the start.</param>
    [HttpGet("terminating-contracts")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<TerminatingContractReportItem>), 200)]
    public async Task<IActionResult> GetTerminatingContractsAsync([FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await staffReportService.GetTerminatingContractsAsync(startDate, endDate,
            CancellationToken);
        return Ok(result);
    }

    /// <summary>Individual Absence: all absences for one staff member over a period.</summary>
    [HttpGet("individual-absence")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<IndividualAbsenceReportItem>), 200)]
    public async Task<IActionResult> GetIndividualAbsenceAsync([FromQuery] Guid staffMemberId,
        [FromQuery] Guid? absenceTypeId = null, [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await staffReportService.GetIndividualAbsenceAsync(staffMemberId, absenceTypeId,
            startDate, endDate, CancellationToken);
        return Ok(result);
    }

    /// <summary>Staff Absence Analysis: absences over a period summarised by service term.</summary>
    [HttpGet("staff-absence-analysis")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<StaffAbsenceAnalysisReportItem>), 200)]
    public async Task<IActionResult> GetStaffAbsenceAnalysisAsync([FromQuery] Guid? absenceTypeId = null,
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await staffReportService.GetStaffAbsenceAnalysisAsync(absenceTypeId, startDate,
            endDate, CancellationToken);
        return Ok(result);
    }

    /// <summary>Long-Term Absence Analysis: extended sickness/maternity absences over the period.</summary>
    [HttpGet("long-term-absence")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<LongTermAbsenceReportItem>), 200)]
    public async Task<IActionResult> GetLongTermAbsenceAsync([FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null, [FromQuery] decimal? minDays = null)
    {
        var result = await staffReportService.GetLongTermAbsenceAsync(startDate, endDate, minDays,
            CancellationToken);
        return Ok(result);
    }

    /// <summary>Staff Training: training records, optionally for one staff member, within the period.</summary>
    [HttpGet("staff-training")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<StaffTrainingReportItem>), 200)]
    public async Task<IActionResult> GetStaffTrainingAsync([FromQuery] Guid? staffMemberId = null,
        [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var result = await staffReportService.GetStaffTrainingAsync(staffMemberId, startDate, endDate,
            CancellationToken);
        return Ok(result);
    }

    /// <summary>Training Course: attendees for one course.</summary>
    [HttpGet("training-course")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingCourseAttendeeReportItem>), 200)]
    public async Task<IActionResult> GetTrainingCourseAttendeesAsync([FromQuery] Guid trainingCourseId)
    {
        var result = await staffReportService.GetTrainingCourseAttendeesAsync(trainingCourseId,
            CancellationToken);
        return Ok(result);
    }

    /// <summary>Active training courses for the Training Course report's picker.</summary>
    [HttpGet("training-courses")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingCourseOption>), 200)]
    public async Task<IActionResult> GetTrainingCourseOptionsAsync()
    {
        var result = await staffReportService.GetTrainingCourseOptionsAsync(CancellationToken);
        return Ok(result);
    }

    /// <summary>Absence types for the absence reports' type filter.</summary>
    [HttpGet("absence-types")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<AbsenceTypeOption>), 200)]
    public async Task<IActionResult> GetAbsenceTypeOptionsAsync()
    {
        var result = await staffReportService.GetAbsenceTypeOptionsAsync(CancellationToken);
        return Ok(result);
    }
}
