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
/// Service terms — the conditions of service a contract is held on, maintained in Staff Setup.
/// Carries the pay-spine settings, default hours and weeks, and the pension schemes on offer.
/// </summary>
public sealed class ServiceTermsController(
    ProblemDetailsFactory problemFactory,
    ILogger<ServiceTermsController> logger,
    IServiceTermService serviceTermService,
    IPayScaleService payScaleService,
    IStaffIncrementService staffIncrementService)
    : BaseApiController(problemFactory, logger)
{
    /// <summary>Get the service terms plus the option lists the editor needs.</summary>
    [HttpGet]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(ServiceTermsResponse), 200)]
    public async Task<IActionResult> GetServiceTermsAsync()
    {
        var result = await serviceTermService.GetServiceTermsAsync(CancellationToken);
        return Ok(result);
    }

    /// <summary>Create a service term.</summary>
    /// <param name="model">The service term to create, with its pension schemes.</param>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateServiceTermAsync([FromBody] ServiceTermUpsertRequest model)
    {
        var id = await serviceTermService.CreateServiceTermAsync(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Update a service term and the pension schemes it offers.</summary>
    /// <param name="serviceTermId">The ServiceTerm id.</param>
    /// <param name="model">The new service term payload.</param>
    [HttpPut("{serviceTermId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateServiceTermAsync([FromRoute] Guid serviceTermId,
        [FromBody] ServiceTermUpsertRequest model)
    {
        await serviceTermService.UpdateServiceTermAsync(serviceTermId, model, CancellationToken);
        return Ok(new IdResponse { Id = serviceTermId });
    }

    /// <summary>Delete a service term. Rejected if any contract or post still uses it.</summary>
    /// <param name="serviceTermId">The ServiceTerm id.</param>
    [HttpDelete("{serviceTermId:guid}")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> DeleteServiceTermAsync([FromRoute] Guid serviceTermId)
    {
        await serviceTermService.DeleteServiceTermAsync(serviceTermId, CancellationToken);
        return Ok(new IdResponse { Id = serviceTermId });
    }

    /// <summary>Get this term's pay spine — its scales, their points, and one generation of salaries.</summary>
    /// <param name="serviceTermId">The ServiceTerm id.</param>
    /// <param name="effectiveFrom">Generation to load. Defaults to the one currently in force.</param>
    [HttpGet("{serviceTermId:guid}/pay")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(ServiceTermPayResponse), 200)]
    public async Task<IActionResult> GetServiceTermPayAsync([FromRoute] Guid serviceTermId,
        [FromQuery] DateTime? effectiveFrom)
    {
        var result = await payScaleService.GetServiceTermPayAsync(serviceTermId, effectiveFrom, CancellationToken);
        return Ok(result);
    }

    /// <summary>Save the term's spine range, its pay scales and the salaries for one generation.</summary>
    /// <param name="serviceTermId">The ServiceTerm id.</param>
    /// <param name="model">The pay setup to save. Scales left out are removed.</param>
    [HttpPut("{serviceTermId:guid}/pay")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateServiceTermPayAsync([FromRoute] Guid serviceTermId,
        [FromBody] ServiceTermPayUpsertRequest model)
    {
        await payScaleService.UpdateServiceTermPayAsync(serviceTermId, model, CancellationToken);
        return Ok(new IdResponse { Id = serviceTermId });
    }

    /// <summary>Calculate a pay award for this term without saving it, so it can be reviewed.</summary>
    /// <param name="serviceTermId">The ServiceTerm id.</param>
    /// <param name="model">The award to model.</param>
    [HttpPost("{serviceTermId:guid}/awards/preview")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(PayAwardPreviewResponse), 200)]
    public async Task<IActionResult> PreviewPayAwardAsync([FromRoute] Guid serviceTermId,
        [FromBody] PayAwardRequest model)
    {
        var result = await payScaleService.PreviewPayAwardAsync(serviceTermId, model, CancellationToken);
        return Ok(result);
    }

    /// <summary>Apply a pay award, closing this term's previous generation and appending the new one.</summary>
    /// <param name="serviceTermId">The ServiceTerm id.</param>
    /// <param name="model">The award to apply.</param>
    [HttpPost("{serviceTermId:guid}/awards")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> ApplyPayAwardAsync([FromRoute] Guid serviceTermId,
        [FromBody] PayAwardRequest model)
    {
        await payScaleService.ApplyPayAwardAsync(serviceTermId, model, CancellationToken);
        return Ok(new IdResponse { Id = serviceTermId });
    }

    /// <summary>Delete a pay scale and its points. Blocked while any contract is held on it.</summary>
    /// <param name="payScaleId">The PayScale id.</param>
    [HttpDelete("payscales/{payScaleId:guid}")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> DeletePayScaleAsync([FromRoute] Guid payScaleId)
    {
        await payScaleService.DeletePayScaleAsync(payScaleId, CancellationToken);
        return Ok(new IdResponse { Id = payScaleId });
    }

    /// <summary>Model the annual increment for this term without saving — who moves up, and to what.</summary>
    /// <param name="serviceTermId">The ServiceTerm id.</param>
    /// <param name="model">The increment's effective date.</param>
    [HttpPost("{serviceTermId:guid}/increment/preview")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IncrementPreviewResponse), 200)]
    public async Task<IActionResult> PreviewIncrementAsync([FromRoute] Guid serviceTermId,
        [FromBody] IncrementPreviewRequest model)
    {
        var result = await staffIncrementService.PreviewAsync(serviceTermId, model, CancellationToken);
        return Ok(result);
    }

    /// <summary>Apply the annual increment now to the chosen contracts, recording each in salary history.</summary>
    /// <param name="serviceTermId">The ServiceTerm id.</param>
    /// <param name="model">The effective date and the contracts to increment.</param>
    [HttpPost("{serviceTermId:guid}/increment")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> ApplyIncrementAsync([FromRoute] Guid serviceTermId,
        [FromBody] IncrementApplyRequest model)
    {
        await staffIncrementService.ApplyAsync(serviceTermId, model, CancellationToken);
        return Ok(new IdResponse { Id = serviceTermId });
    }

    /// <summary>Schedule the increment for a future date; applied when due, re-computed then.</summary>
    /// <param name="serviceTermId">The ServiceTerm id.</param>
    /// <param name="model">The date it should take effect.</param>
    [HttpPost("{serviceTermId:guid}/increment/schedule")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> ScheduleIncrementAsync([FromRoute] Guid serviceTermId,
        [FromBody] IncrementScheduleRequest model)
    {
        var id = await staffIncrementService.ScheduleAsync(serviceTermId, model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Every scheduled increment for a service term (pending and past).</summary>
    /// <param name="serviceTermId">The ServiceTerm id.</param>
    [HttpGet("{serviceTermId:guid}/increment/scheduled")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<ScheduledIncrementResponse>), 200)]
    public async Task<IActionResult> GetScheduledIncrementsAsync([FromRoute] Guid serviceTermId)
    {
        var result = await staffIncrementService.GetScheduledAsync(serviceTermId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Pending increments whose date has arrived, across all terms — the due worklist.</summary>
    [HttpGet("increment/due")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<ScheduledIncrementResponse>), 200)]
    public async Task<IActionResult> GetDueIncrementsAsync()
    {
        var result = await staffIncrementService.GetDueAsync(CancellationToken);
        return Ok(result);
    }

    /// <summary>Run a scheduled increment now, applying to everyone eligible and marking it complete.</summary>
    /// <param name="scheduledId">The ScheduledIncrement id.</param>
    [HttpPost("increment/scheduled/{scheduledId:guid}/apply")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> ApplyScheduledIncrementAsync([FromRoute] Guid scheduledId)
    {
        await staffIncrementService.ApplyScheduledAsync(scheduledId, CancellationToken);
        return Ok(new IdResponse { Id = scheduledId });
    }

    /// <summary>Cancel a pending scheduled increment.</summary>
    /// <param name="scheduledId">The ScheduledIncrement id.</param>
    [HttpDelete("increment/scheduled/{scheduledId:guid}")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CancelScheduledIncrementAsync([FromRoute] Guid scheduledId)
    {
        await staffIncrementService.CancelScheduledAsync(scheduledId, CancellationToken);
        return Ok(new IdResponse { Id = scheduledId });
    }
}
