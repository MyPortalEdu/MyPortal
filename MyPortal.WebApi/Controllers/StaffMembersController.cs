using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.People;
using MyPortal.Services.Interfaces.People;
using MyPortal.WebApi.Infrastructure.Attributes;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Manage individual staff-member records — the core staff details plus the
/// underlying person's biographical fields. Employment, contract,
/// DBS/right-to-work and qualification records hang off a staff member and are
/// managed through their own endpoints. Use <c>GET /api/people/staff</c> for the
/// lightweight staff picker.
/// </summary>
public sealed class StaffMembersController : BaseApiController
{
    private readonly IStaffMemberService _staffMemberService;

    public StaffMembersController(ProblemDetailsFactory problemFactory, ILogger<StaffMembersController> logger,
        IStaffMemberService staffMemberService) : base(problemFactory, logger)
    {
        _staffMemberService = staffMemberService;
    }

    /// <summary>Get a staff member's core + biographical details by id.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}")]
    [UserType(UserType.Staff)]
    // Coarse gate (the attribute is global; relationship scoping happens in the service).
    // Edit implies read so an editor can load the record they're about to change. When the
    // resolver lands this should widen to include the Own/Managed view scopes so self/managed
    // viewers reach the service, which then does the per-(viewer,subject) check.
    [Permission(PermissionMode.RequireAny,
        Permissions.Staff.ViewAllStaffBasicDetails,
        Permissions.Staff.EditAllStaffBasicDetails)]
    [ProducesResponseType(typeof(StaffMemberDetailsResponse), 200)]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid staffMemberId)
    {
        var result = await _staffMemberService.GetDetailsAsync(staffMemberId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Create a new staff member (and the underlying person).</summary>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Staff.EditAllStaffBasicDetails)]
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
    [Permission(PermissionMode.RequireAny, Permissions.Staff.EditAllStaffBasicDetails)]
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
    [Permission(PermissionMode.RequireAny, Permissions.Staff.EditAllStaffBasicDetails)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid staffMemberId)
    {
        await _staffMemberService.DeleteAsync(staffMemberId, CancellationToken);
        return NoContent();
    }
}
