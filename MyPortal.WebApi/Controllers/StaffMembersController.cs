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
/// Manage individual staff-member records. <see cref="GetHeaderAsync"/> returns the identity
/// header + the viewer's relationship to the subject; per-section bodies are fetched through
/// their own endpoints, each gated server-side via <c>IStaffMemberAccessService</c>. Use
/// <c>GET /api/people/staff</c> for the lightweight staff picker.
/// </summary>
/// <remarks>
/// Permission gating lives in the service layer (the access resolver for reads, plain
/// <c>RequirePermissionAsync</c> for writes). The controller deliberately doesn't carry
/// <c>[Permission]</c> attributes so the service stays the single source of truth and the
/// two can't drift.
/// </remarks>
public sealed class StaffMembersController : BaseApiController
{
    private readonly IStaffMemberService _staffMemberService;

    public StaffMembersController(ProblemDetailsFactory problemFactory, ILogger<StaffMembersController> logger,
        IStaffMemberService staffMemberService) : base(problemFactory, logger)
    {
        _staffMemberService = staffMemberService;
    }

    /// <summary>Get the staff profile header for a given staff member id.</summary>
    /// <remarks>
    /// Returns identity + status + the viewer's <c>Relationship</c> to the subject. The FE
    /// composes the sidebar from <c>Relationship</c> plus its own permission claim. 403 if the
    /// viewer holds no scope of <c>ViewStaffBasicDetails</c> covering this subject; 404 if the
    /// staff member doesn't exist (only reachable by All-scope holders, so existence isn't leaked).
    /// </remarks>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffMemberHeaderResponse), 200)]
    public async Task<IActionResult> GetHeaderAsync([FromRoute] Guid staffMemberId)
    {
        var result = await _staffMemberService.GetHeaderAsync(staffMemberId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Create a new staff member (and the underlying person).</summary>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateAsync([FromBody] StaffMemberUpsertRequest model)
    {
        var id = await _staffMemberService.CreateAsync(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Update a staff member's core + biographical details.</summary>
    /// <param name="staffMemberId">The StaffMember id to update.</param>
    /// <param name="model">The new details.</param>
    [HttpPut("{staffMemberId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid staffMemberId,
        [FromBody] StaffMemberUpsertRequest model)
    {
        await _staffMemberService.UpdateAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Soft-delete a staff member.</summary>
    /// <param name="staffMemberId">The StaffMember id to delete.</param>
    [HttpDelete("{staffMemberId:guid}")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid staffMemberId)
    {
        await _staffMemberService.DeleteAsync(staffMemberId, CancellationToken);
        return NoContent();
    }
}
