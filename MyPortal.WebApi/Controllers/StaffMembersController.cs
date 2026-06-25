using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.People;
using MyPortal.WebApi.Infrastructure.Attributes;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Manage individual staff-member records. <see cref="GetHeaderAsync"/> returns the identity
/// header + the viewer's relationship to the subject; per-section bodies are fetched through
/// their own endpoints (currently: basic details), each gated server-side via
/// <c>IStaffMemberAccessService</c>. Use <c>GET /api/people/staff</c> for the lightweight
/// staff picker.
/// </summary>
/// <remarks>
/// Permission gating lives in the service layer (the access resolver for per-subject reads/writes,
/// plain <c>RequirePermissionAsync</c> for non-relationship-scoped actions like Create / Delete).
/// The controller deliberately doesn't carry <c>[Permission]</c> attributes so the service stays
/// the single source of truth and the two can't drift.
///
/// The staff <b>Documents</b> area is served by the inherited attachment endpoints
/// (<c>{staffMemberId}/attachments/...</c>) from <see cref="BaseDirectoryEntityController{Person}"/>;
/// the route's entityId is the staff member id, which <see cref="IStaffAttachmentsService"/> resolves
/// to the owning Person's directory tree and gates on the staff Documents permission domain.
/// </remarks>
public sealed class StaffMembersController : BaseDirectoryEntityController<Person>
{
    private readonly IStaffMemberService _staffMemberService;
    private readonly IStaffContactService _staffContactService;
    private readonly IStaffAddressService _staffAddressService;

    public StaffMembersController(ProblemDetailsFactory problemFactory, ILogger<StaffMembersController> logger,
        IStaffMemberService staffMemberService, IStaffContactService staffContactService,
        IStaffAddressService staffAddressService, IStaffAttachmentsService staffAttachmentsService)
        : base(problemFactory, logger, staffAttachmentsService)
    {
        _staffMemberService = staffMemberService;
        _staffContactService = staffContactService;
        _staffAddressService = staffAddressService;
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

    /// <summary>Basic details area — person bio (sans equality fields) + staff code.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/basic-details")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffBasicDetailsResponse), 200)]
    public async Task<IActionResult> GetBasicDetailsAsync([FromRoute] Guid staffMemberId)
    {
        var result = await _staffMemberService.GetBasicDetailsAsync(staffMemberId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Update the basic details area. Does not touch equality, employment, or
    /// professional fields — each of those has its own endpoint.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    /// <param name="model">The new basic-details payload.</param>
    [HttpPut("{staffMemberId:guid}/basic-details")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateBasicDetailsAsync([FromRoute] Guid staffMemberId,
        [FromBody] StaffBasicDetailsUpsertRequest model)
    {
        await _staffMemberService.UpdateBasicDetailsAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Contact Details area — the staff member's owned emails and phone numbers, plus
    /// the type options for the editor. Shared addresses arrive in a later slice.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/contact-details")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffContactDetailsResponse), 200)]
    public async Task<IActionResult> GetContactDetailsAsync([FromRoute] Guid staffMemberId)
    {
        var result = await _staffContactService.GetContactDetailsAsync(staffMemberId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Replace the Contact Details area. Each list is a whole-collection replace — the
    /// server inserts new rows, updates matched ids, and soft-deletes anything dropped.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    /// <param name="model">The new emails and phone numbers.</param>
    [HttpPut("{staffMemberId:guid}/contact-details")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateContactDetailsAsync([FromRoute] Guid staffMemberId,
        [FromBody] StaffContactDetailsUpsertRequest model)
    {
        await _staffContactService.UpdateContactDetailsAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Addresses section of Contact Details — a staff member's linked addresses + type
    /// options. Addresses are shared entities, so writes are granular rather than a bulk replace.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/addresses")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(AddressListResponse), 200)]
    public async Task<IActionResult> GetAddressesAsync([FromRoute] Guid staffMemberId)
    {
        var result = await _staffAddressService.GetAddressesAsync(staffMemberId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Search existing addresses for the search-before-add flow (avoids duplicate rows for
    /// people who share an address). Blank / too-short queries return an empty list.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    /// <param name="query">Postcode / street / building fragment to search for.</param>
    [HttpGet("{staffMemberId:guid}/address-matches")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<AddressMatchResponse>), 200)]
    public async Task<IActionResult> SearchAddressMatchesAsync([FromRoute] Guid staffMemberId,
        [FromQuery] string? query)
    {
        var result = await _staffAddressService.SearchAddressesAsync(staffMemberId, query, CancellationToken);
        return Ok(result);
    }

    /// <summary>Add an address to a staff member — link an existing one or create a new one.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    /// <param name="model">The address to link/create, with type and main flag.</param>
    [HttpPost("{staffMemberId:guid}/addresses")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> AddAddressAsync([FromRoute] Guid staffMemberId,
        [FromBody] PersonAddressUpsertRequest model)
    {
        var addressPersonId = await _staffAddressService.AddAddressAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = addressPersonId });
    }

    /// <summary>Update an address link. When the address is shared, <c>Mode</c> decides whether the
    /// edit applies to everyone (FixInPlace) or forks a new address for this person (Moved).</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    /// <param name="addressPersonId">The address-link id (from the address list).</param>
    /// <param name="model">The updated address + link details and edit mode.</param>
    [HttpPut("{staffMemberId:guid}/addresses/{addressPersonId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateAddressAsync([FromRoute] Guid staffMemberId,
        [FromRoute] Guid addressPersonId, [FromBody] PersonAddressUpdateRequest model)
    {
        await _staffAddressService.UpdateAddressAsync(staffMemberId, addressPersonId, model, CancellationToken);
        return Ok(new IdResponse { Id = addressPersonId });
    }

    /// <summary>Unlink an address from a staff member. The shared address itself is left for anyone
    /// else linked to it.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    /// <param name="addressPersonId">The address-link id to remove.</param>
    [HttpDelete("{staffMemberId:guid}/addresses/{addressPersonId:guid}")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RemoveAddressAsync([FromRoute] Guid staffMemberId,
        [FromRoute] Guid addressPersonId)
    {
        await _staffAddressService.RemoveAddressAsync(staffMemberId, addressPersonId, CancellationToken);
        return NoContent();
    }

    /// <summary>Create a new staff member with basic details only. Equality, employment,
    /// professional, etc. are populated post-creation via their area PUTs.</summary>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateAsync([FromBody] StaffBasicDetailsUpsertRequest model)
    {
        var id = await _staffMemberService.CreateAsync(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Search existing People for the new-staff-member flow.</summary>
    /// <remarks>
    /// HR searches existing People (any subtype) so a joiner already on file — a contact, agent,
    /// former student — gets a staff role attached to their existing Person rather than a
    /// duplicate. Results flag anyone who is already staff (<c>ExistingStaffMemberId</c>). Gated
    /// in the service on the create scope. Blank / too-short queries return an empty list.
    /// </remarks>
    /// <param name="query">The name fragment to search for.</param>
    [HttpGet("person-matches")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<PersonMatchResponse>), 200)]
    public async Task<IActionResult> SearchPeopleAsync([FromQuery] string? query)
    {
        var result = await _staffMemberService.SearchPeopleAsync(query, CancellationToken);
        return Ok(result);
    }

    /// <summary>Attach a staff role to an existing Person (no new Person row).</summary>
    /// <remarks>
    /// Used by the create flow when HR picks an existing person. Only the staff code is supplied —
    /// the person's bio is left untouched. 404 if the person doesn't exist; 400 if they're already
    /// staff.
    /// </remarks>
    [HttpPost("for-person")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateForPersonAsync([FromBody] StaffMemberCreateForPersonRequest model)
    {
        var id = await _staffMemberService.CreateForPersonAsync(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
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
