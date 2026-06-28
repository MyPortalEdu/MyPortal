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
/// Staff-member detail endpoints.
/// </summary>
/// <remarks>
/// Use <c>GET /api/people/staff</c> for the lightweight staff picker.
/// </remarks>
public sealed class StaffMembersController : BaseDirectoryEntityController<Person>
{
    private readonly IStaffMemberService _staffMemberService;
    private readonly IStaffContactService _staffContactService;
    private readonly IStaffAddressService _staffAddressService;
    private readonly IStaffEqualityService _staffEqualityService;
    private readonly IStaffProfessionalService _staffProfessionalService;
    private readonly IStaffEmploymentService _staffEmploymentService;
    private readonly IStaffPreEmploymentService _staffPreEmploymentService;

    public StaffMembersController(ProblemDetailsFactory problemFactory, ILogger<StaffMembersController> logger,
        IStaffMemberService staffMemberService, IStaffContactService staffContactService,
        IStaffAddressService staffAddressService, IStaffEqualityService staffEqualityService,
        IStaffProfessionalService staffProfessionalService, IStaffEmploymentService staffEmploymentService,
        IStaffPreEmploymentService staffPreEmploymentService, IStaffAttachmentsService staffAttachmentsService)
        : base(problemFactory, logger, staffAttachmentsService)
    {
        _staffMemberService = staffMemberService;
        _staffContactService = staffContactService;
        _staffAddressService = staffAddressService;
        _staffEqualityService = staffEqualityService;
        _staffProfessionalService = staffProfessionalService;
        _staffEmploymentService = staffEmploymentService;
        _staffPreEmploymentService = staffPreEmploymentService;
    }

    /// <summary>Get the staff profile header.</summary>
    /// <remarks>
    /// Returns identity, status, and the viewer's <c>Relationship</c> to the subject. Returns 403
    /// if the viewer lacks access and 404 if the staff member does not exist.
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

    /// <summary>Get the basic details area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/basic-details")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffBasicDetailsResponse), 200)]
    public async Task<IActionResult> GetBasicDetailsAsync([FromRoute] Guid staffMemberId)
    {
        var result = await _staffMemberService.GetBasicDetailsAsync(staffMemberId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Update the basic details area.</summary>
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

    /// <summary>Get the contact details area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/contact-details")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffContactDetailsResponse), 200)]
    public async Task<IActionResult> GetContactDetailsAsync([FromRoute] Guid staffMemberId)
    {
        var result = await _staffContactService.GetContactDetailsAsync(staffMemberId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Replace the contact details area.</summary>
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

    /// <summary>Get the equality details area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/equality-details")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffEqualityDetailsResponse), 200)]
    public async Task<IActionResult> GetEqualityDetailsAsync([FromRoute] Guid staffMemberId)
    {
        var result = await _staffEqualityService.GetEqualityDetailsAsync(staffMemberId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Update the equality details area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    /// <param name="model">The new equality payload.</param>
    [HttpPut("{staffMemberId:guid}/equality-details")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateEqualityDetailsAsync([FromRoute] Guid staffMemberId,
        [FromBody] StaffEqualityDetailsUpsertRequest model)
    {
        await _staffEqualityService.UpdateEqualityDetailsAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Get the professional details area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/professional-details")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffProfessionalDetailsResponse), 200)]
    public async Task<IActionResult> GetProfessionalDetailsAsync([FromRoute] Guid staffMemberId)
    {
        var result = await _staffProfessionalService.GetProfessionalDetailsAsync(staffMemberId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Update the professional details area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    /// <param name="model">The new professional-details payload.</param>
    [HttpPut("{staffMemberId:guid}/professional-details")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateProfessionalDetailsAsync([FromRoute] Guid staffMemberId,
        [FromBody] StaffProfessionalDetailsUpsertRequest model)
    {
        await _staffProfessionalService.UpdateProfessionalDetailsAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Get the employment details area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/employment-details")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffEmploymentDetailsResponse), 200)]
    public async Task<IActionResult> GetEmploymentDetailsAsync([FromRoute] Guid staffMemberId)
    {
        var result = await _staffEmploymentService.GetEmploymentDetailsAsync(staffMemberId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Update the employment details area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    /// <param name="model">The new employment-details payload.</param>
    [HttpPut("{staffMemberId:guid}/employment-details")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateEmploymentDetailsAsync([FromRoute] Guid staffMemberId,
        [FromBody] StaffEmploymentDetailsUpsertRequest model)
    {
        await _staffEmploymentService.UpdateEmploymentDetailsAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Get the pre-employment checks area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/pre-employment-checks")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffPreEmploymentChecksResponse), 200)]
    public async Task<IActionResult> GetPreEmploymentChecksAsync([FromRoute] Guid staffMemberId)
    {
        var result = await _staffPreEmploymentService.GetPreEmploymentChecksAsync(staffMemberId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Update the pre-employment checks area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    /// <param name="model">The new pre-employment-checks payload.</param>
    [HttpPut("{staffMemberId:guid}/pre-employment-checks")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdatePreEmploymentChecksAsync([FromRoute] Guid staffMemberId,
        [FromBody] StaffPreEmploymentChecksUpsertRequest model)
    {
        await _staffPreEmploymentService.UpdatePreEmploymentChecksAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Get the addresses section.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/addresses")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(AddressListResponse), 200)]
    public async Task<IActionResult> GetAddressesAsync([FromRoute] Guid staffMemberId)
    {
        var result = await _staffAddressService.GetAddressesAsync(staffMemberId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Search for existing addresses to add.</summary>
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

    /// <summary>Add an address to a staff member.</summary>
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

    /// <summary>Update an address link.</summary>
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

    /// <summary>Unlink an address from a staff member.</summary>
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

    /// <summary>Create a new staff member.</summary>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateAsync([FromBody] StaffBasicDetailsUpsertRequest model)
    {
        var id = await _staffMemberService.CreateAsync(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Search for existing people to attach a staff role to.</summary>
    /// <remarks>
    /// Results include anyone already staff via <c>ExistingStaffMemberId</c>. Blank or too-short
    /// queries return an empty list.
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

    /// <summary>Attach a staff role to an existing person.</summary>
    /// <remarks>
    /// Only the staff code is supplied. Returns 404 if the person does not exist and 400 if they
    /// are already staff.
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
