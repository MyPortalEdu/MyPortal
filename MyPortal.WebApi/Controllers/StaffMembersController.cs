using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;
using MyPortal.Auth.Attributes;
using MyPortal.Common.Constants;
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
public sealed class StaffMembersController(
    ProblemDetailsFactory problemFactory,
    ILogger<StaffMembersController> logger,
    IStaffMemberService staffMemberService,
    IStaffContactService staffContactService,
    IStaffAddressService staffAddressService,
    IStaffEqualityService staffEqualityService,
    IStaffProfessionalService staffProfessionalService,
    IStaffEmploymentService staffEmploymentService,
    IStaffPreEmploymentService staffPreEmploymentService,
    IStaffAbsenceService staffAbsenceService,
    IStaffTimetableService staffTimetableService,
    IStaffPerformanceService staffPerformanceService,
    IStaffAttachmentsService staffAttachmentsService)
    : BaseDirectoryEntityController<Person>(problemFactory, logger, staffAttachmentsService)
{
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
        var result = await staffMemberService.GetHeaderAsync(staffMemberId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Get the basic details area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/basic-details")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffBasicDetailsResponse), 200)]
    public async Task<IActionResult> GetBasicDetailsAsync([FromRoute] Guid staffMemberId)
    {
        var result = await staffMemberService.GetBasicDetailsAsync(staffMemberId, CancellationToken);
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
        await staffMemberService.UpdateBasicDetailsAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Add or replace the staff member's photo. The image is resized server-side.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    /// <param name="file">The uploaded image (multipart form field <c>file</c>).</param>
    [HttpPut("{staffMemberId:guid}/photo")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> SetPhotoAsync([FromRoute] Guid staffMemberId, IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequestProblem("No file was uploaded.");
        }

        await using var stream = file.OpenReadStream();
        await staffMemberService.SetPhotoAsync(staffMemberId, stream, file.ContentType, file.FileName,
            CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Stream the staff member's photo. 404 if none.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/photo")]
    [UserType(UserType.Staff)]
    public async Task<IActionResult> GetPhotoAsync([FromRoute] Guid staffMemberId)
    {
        var content = await staffMemberService.GetPhotoAsync(staffMemberId, CancellationToken);

        var typedHeaders = Response.GetTypedHeaders();
        if (!string.IsNullOrWhiteSpace(content.Details.Hash))
        {
            typedHeaders.ETag = new EntityTagHeaderValue($"\"{content.Details.Hash}\"");
        }
        typedHeaders.LastModified = content.Details.LastModifiedAt;
        Response.Headers["X-Content-Type-Options"] = "nosniff";

        var safeContentType = SafeContentTypes.Sanitize(content.Details.ContentType);
        return File(content.Content, safeContentType, content.Details.FileName,
            enableRangeProcessing: content.Content.CanSeek);
    }

    /// <summary>Remove the staff member's photo.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpDelete("{staffMemberId:guid}/photo")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> DeletePhotoAsync([FromRoute] Guid staffMemberId)
    {
        await staffMemberService.DeletePhotoAsync(staffMemberId, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Get the contact details area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/contact-details")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffContactDetailsResponse), 200)]
    public async Task<IActionResult> GetContactDetailsAsync([FromRoute] Guid staffMemberId)
    {
        var result = await staffContactService.GetContactDetailsAsync(staffMemberId, CancellationToken);
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
        await staffContactService.UpdateContactDetailsAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Get the equality details area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/equality-details")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffEqualityDetailsResponse), 200)]
    public async Task<IActionResult> GetEqualityDetailsAsync([FromRoute] Guid staffMemberId)
    {
        var result = await staffEqualityService.GetEqualityDetailsAsync(staffMemberId, CancellationToken);
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
        await staffEqualityService.UpdateEqualityDetailsAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Get the professional details area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/professional-details")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffProfessionalDetailsResponse), 200)]
    public async Task<IActionResult> GetProfessionalDetailsAsync([FromRoute] Guid staffMemberId)
    {
        var result = await staffProfessionalService.GetProfessionalDetailsAsync(staffMemberId, CancellationToken);
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
        await staffProfessionalService.UpdateProfessionalDetailsAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Get the employment details area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/employment-details")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffEmploymentDetailsResponse), 200)]
    public async Task<IActionResult> GetEmploymentDetailsAsync([FromRoute] Guid staffMemberId)
    {
        var result = await staffEmploymentService.GetEmploymentDetailsAsync(staffMemberId, CancellationToken);
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
        await staffEmploymentService.UpdateEmploymentDetailsAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Get the pre-employment checks area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/pre-employment-checks")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffPreEmploymentChecksResponse), 200)]
    public async Task<IActionResult> GetPreEmploymentChecksAsync([FromRoute] Guid staffMemberId)
    {
        var result = await staffPreEmploymentService.GetPreEmploymentChecksAsync(staffMemberId, CancellationToken);
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
        await staffPreEmploymentService.UpdatePreEmploymentChecksAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Get the absences &amp; leave area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/absences")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffAbsencesResponse), 200)]
    public async Task<IActionResult> GetAbsencesAsync([FromRoute] Guid staffMemberId)
    {
        var result = await staffAbsenceService.GetAbsencesAsync(staffMemberId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Update the absences &amp; leave area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    /// <param name="model">The new absences payload.</param>
    [HttpPut("{staffMemberId:guid}/absences")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateAbsencesAsync([FromRoute] Guid staffMemberId,
        [FromBody] StaffAbsencesUpsertRequest model)
    {
        await staffAbsenceService.UpdateAbsencesAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Get the performance (appraisal) area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/performance")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffPerformanceResponse), 200)]
    public async Task<IActionResult> GetPerformanceAsync([FromRoute] Guid staffMemberId)
    {
        var result = await staffPerformanceService.GetPerformanceAsync(staffMemberId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Update the performance (appraisal) area.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    /// <param name="model">The new performance payload.</param>
    [HttpPut("{staffMemberId:guid}/performance")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdatePerformanceAsync([FromRoute] Guid staffMemberId,
        [FromBody] StaffPerformanceUpsertRequest model)
    {
        await staffPerformanceService.UpdatePerformanceAsync(staffMemberId, model, CancellationToken);
        return Ok(new IdResponse { Id = staffMemberId });
    }

    /// <summary>Get the timetable / calendar area for a date window.</summary>
    /// <remarks>
    /// Returns every calendar entry overlapping [<paramref name="from"/>, <paramref name="to"/>) the
    /// viewer may see — lessons, detentions, diary events, non-contact periods, parent-evening
    /// appointments and (when permitted) absences. Read-only.
    /// </remarks>
    /// <param name="staffMemberId">The StaffMember id.</param>
    /// <param name="from">Inclusive window start.</param>
    /// <param name="to">Exclusive window end.</param>
    [HttpGet("{staffMemberId:guid}/timetable")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(StaffCalendarResponse), 200)]
    public async Task<IActionResult> GetTimetableAsync([FromRoute] Guid staffMemberId,
        [FromQuery] DateTime from, [FromQuery] DateTime to)
    {
        var result = await staffTimetableService.GetCalendarAsync(staffMemberId, from, to, CancellationToken);
        return Ok(result);
    }

    /// <summary>Get the addresses section.</summary>
    /// <param name="staffMemberId">The StaffMember id.</param>
    [HttpGet("{staffMemberId:guid}/addresses")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(AddressListResponse), 200)]
    public async Task<IActionResult> GetAddressesAsync([FromRoute] Guid staffMemberId)
    {
        var result = await staffAddressService.GetAddressesAsync(staffMemberId, CancellationToken);
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
        var result = await staffAddressService.SearchAddressesAsync(staffMemberId, query, CancellationToken);
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
        var addressPersonId = await staffAddressService.AddAddressAsync(staffMemberId, model, CancellationToken);
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
        await staffAddressService.UpdateAddressAsync(staffMemberId, addressPersonId, model, CancellationToken);
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
        await staffAddressService.RemoveAddressAsync(staffMemberId, addressPersonId, CancellationToken);
        return NoContent();
    }

    /// <summary>Create a new staff member.</summary>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateAsync([FromBody] StaffBasicDetailsUpsertRequest model)
    {
        var id = await staffMemberService.CreateAsync(model, CancellationToken);
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
        var result = await staffMemberService.SearchPeopleAsync(query, CancellationToken);
        return Ok(result);
    }

    /// <summary>Advisory check for whether a staff code is available (not already in use).</summary>
    /// <remarks>
    /// For inline UI feedback only — the authoritative uniqueness guard still runs on create/update.
    /// Blank codes report available. Pass <paramref name="excludeId"/> when editing.
    /// </remarks>
    [HttpGet("code-available")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(AvailabilityResponse), 200)]
    public async Task<IActionResult> IsCodeAvailableAsync([FromQuery] string? code, [FromQuery] Guid? excludeId)
    {
        var available = await staffMemberService.IsCodeAvailableAsync(code, excludeId, CancellationToken);
        return Ok(new AvailabilityResponse { Available = available });
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
        var id = await staffMemberService.CreateForPersonAsync(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Soft-delete a staff member.</summary>
    /// <param name="staffMemberId">The StaffMember id to delete.</param>
    [HttpDelete("{staffMemberId:guid}")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid staffMemberId)
    {
        await staffMemberService.DeleteAsync(staffMemberId, CancellationToken);
        return NoContent();
    }
}
