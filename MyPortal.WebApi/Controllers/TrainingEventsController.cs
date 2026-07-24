using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.People;
using MyPortal.Services.Interfaces.People;
using MyPortal.WebApi.Infrastructure.Attributes;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Training events — scheduled deliveries of a training course that staff are booked onto. Each is a
/// diary event (so it appears on calendars); marking an attendee as attended issues a certificate.
/// HR (Professional details) only.
/// </summary>
public sealed class TrainingEventsController(
    ProblemDetailsFactory problemFactory,
    ILogger<TrainingEventsController> logger,
    ITrainingEventService trainingEventService)
    : BaseApiController(problemFactory, logger)
{
    /// <summary>List training events overlapping the period (defaults to ±6 months).</summary>
    [HttpGet]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingEventSummaryResponse>), 200)]
    public async Task<IActionResult> ListAsync([FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var result = await trainingEventService.ListAsync(from, to, CancellationToken);
        return Ok(result);
    }

    /// <summary>Get a training event with its attendee roster.</summary>
    [HttpGet("{id:guid}")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(TrainingEventDetailsResponse), 200)]
    public async Task<IActionResult> GetAsync([FromRoute] Guid id)
    {
        var result = await trainingEventService.GetAsync(id, CancellationToken);
        return result == null ? NotFound() : Ok(result);
    }

    /// <summary>Schedule a training event.</summary>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateAsync([FromBody] TrainingEventUpsertRequest model)
    {
        var id = await trainingEventService.CreateAsync(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Update a training event.</summary>
    [HttpPut("{id:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id,
        [FromBody] TrainingEventUpsertRequest model)
    {
        await trainingEventService.UpdateAsync(id, model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Cancel a training event (removes it, its attendees and their certificates).</summary>
    [HttpDelete("{id:guid}")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
    {
        await trainingEventService.DeleteAsync(id, CancellationToken);
        return NoContent();
    }

    /// <summary>Book one or more staff onto a training event.</summary>
    [HttpPost("{id:guid}/attendees")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> BookAttendeesAsync([FromRoute] Guid id,
        [FromBody] BookTrainingAttendeesRequest model)
    {
        await trainingEventService.BookAttendeesAsync(id, model.StaffMemberIds, CancellationToken);
        return NoContent();
    }

    /// <summary>Remove an attendee from a training event.</summary>
    [HttpDelete("{id:guid}/attendees/{staffMemberId:guid}")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RemoveAttendeeAsync([FromRoute] Guid id,
        [FromRoute] Guid staffMemberId)
    {
        await trainingEventService.RemoveAttendeeAsync(id, staffMemberId, CancellationToken);
        return NoContent();
    }

    /// <summary>Mark an attendee attended (issues a certificate) or not.</summary>
    [HttpPut("{id:guid}/attendees/{staffMemberId:guid}/attendance")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> SetAttendanceAsync([FromRoute] Guid id,
        [FromRoute] Guid staffMemberId, [FromBody] SetTrainingAttendanceRequest model)
    {
        await trainingEventService.SetAttendanceAsync(id, staffMemberId, model.Attended, CancellationToken);
        return NoContent();
    }
}
