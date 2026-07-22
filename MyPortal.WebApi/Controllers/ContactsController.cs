using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Constants;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.People;
using MyPortal.Services.Interfaces.People;
using MyPortal.WebApi.Infrastructure.Attributes;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Contact-record detail endpoints (the staff-facing contact profile for guardians/carers/emergency
/// contacts). Access is flat / role-based via the <c>[Permission]</c> attribute — a granted staff
/// viewer sees any contact.
/// </summary>
[UserType(UserType.Staff)]
public sealed class ContactsController(
    ProblemDetailsFactory problemFactory,
    ILogger<ContactsController> logger,
    IContactService contactService)
    : BaseApiController(problemFactory, logger)
{
    /// <summary>Page through contact summaries for the contact list / picker.</summary>
    [HttpGet]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.ViewContactDetails)]
    [ProducesResponseType(typeof(PageResult<ContactSummaryResponse>), 200)]
    public async Task<IActionResult> GetContactsAsync([FromQuery] int page, [FromQuery] int pageSize,
        [FromQuery] FilterOptions? filter, [FromQuery] SortOptions? sort)
    {
        var options = GetListingOptions(page, pageSize, filter, sort);

        var result = await contactService.GetContactsAsync(options.FilterOptions, options.SortOptions,
            options.PageOptions, CancellationToken);

        return Ok(result);
    }

    /// <summary>Get the contact profile header (identity + photo). 404 if not found.</summary>
    /// <param name="contactId">The Contact id.</param>
    [HttpGet("{contactId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.ViewContactDetails)]
    [ProducesResponseType(typeof(ContactHeaderResponse), 200)]
    public async Task<IActionResult> GetHeaderAsync([FromRoute] Guid contactId)
    {
        var result = await contactService.GetHeaderAsync(contactId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Get the contact's associated students (read-only reverse view of the relationship).</summary>
    /// <param name="contactId">The Contact id.</param>
    [HttpGet("{contactId:guid}/students")]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.ViewContactDetails)]
    [ProducesResponseType(typeof(IReadOnlyList<ContactStudentResponse>), 200)]
    public async Task<IActionResult> GetAssociatedStudentsAsync([FromRoute] Guid contactId)
    {
        var result = await contactService.GetAssociatedStudentsAsync(contactId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Get the basic details area.</summary>
    /// <param name="contactId">The Contact id.</param>
    [HttpGet("{contactId:guid}/basic-details")]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.ViewContactDetails)]
    [ProducesResponseType(typeof(ContactBasicDetailsResponse), 200)]
    public async Task<IActionResult> GetBasicDetailsAsync([FromRoute] Guid contactId)
    {
        var result = await contactService.GetBasicDetailsAsync(contactId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Update the basic details area.</summary>
    /// <param name="contactId">The Contact id.</param>
    /// <param name="model">The new basic-details payload.</param>
    [HttpPut("{contactId:guid}/basic-details")]
    [ValidateModel]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.EditContactDetails)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateBasicDetailsAsync([FromRoute] Guid contactId,
        [FromBody] ContactBasicDetailsUpsertRequest model)
    {
        await contactService.UpdateBasicDetailsAsync(contactId, model, CancellationToken);
        return Ok(new IdResponse { Id = contactId });
    }

    /// <summary>Get the contact details area (emails + phone numbers).</summary>
    /// <param name="contactId">The Contact id.</param>
    [HttpGet("{contactId:guid}/contact-details")]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.ViewContactDetails)]
    [ProducesResponseType(typeof(PersonContactDetailsResponse), 200)]
    public async Task<IActionResult> GetContactDetailsAsync([FromRoute] Guid contactId)
    {
        var result = await contactService.GetContactDetailsAsync(contactId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Replace the contact details area.</summary>
    /// <param name="contactId">The Contact id.</param>
    /// <param name="model">The new emails and phone numbers.</param>
    [HttpPut("{contactId:guid}/contact-details")]
    [ValidateModel]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.EditContactDetails)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateContactDetailsAsync([FromRoute] Guid contactId,
        [FromBody] PersonContactDetailsUpsertRequest model)
    {
        await contactService.UpdateContactDetailsAsync(contactId, model, CancellationToken);
        return Ok(new IdResponse { Id = contactId });
    }

    /// <summary>Get the addresses section.</summary>
    /// <param name="contactId">The Contact id.</param>
    [HttpGet("{contactId:guid}/addresses")]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.ViewContactDetails)]
    [ProducesResponseType(typeof(AddressListResponse), 200)]
    public async Task<IActionResult> GetAddressesAsync([FromRoute] Guid contactId)
    {
        var result = await contactService.GetAddressesAsync(contactId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Search for existing addresses to add.</summary>
    /// <param name="contactId">The Contact id.</param>
    /// <param name="query">Postcode / street / building fragment to search for.</param>
    [HttpGet("{contactId:guid}/address-matches")]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.EditContactDetails)]
    [ProducesResponseType(typeof(IReadOnlyList<AddressMatchResponse>), 200)]
    public async Task<IActionResult> SearchAddressMatchesAsync([FromRoute] Guid contactId,
        [FromQuery] string? query)
    {
        var result = await contactService.SearchAddressesAsync(query, CancellationToken);
        return Ok(result);
    }

    /// <summary>Add an address to a contact.</summary>
    /// <param name="contactId">The Contact id.</param>
    /// <param name="model">The address to link/create, with type and main flag.</param>
    [HttpPost("{contactId:guid}/addresses")]
    [ValidateModel]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.EditContactDetails)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> AddAddressAsync([FromRoute] Guid contactId,
        [FromBody] PersonAddressUpsertRequest model)
    {
        var addressPersonId = await contactService.AddAddressAsync(contactId, model, CancellationToken);
        return Ok(new IdResponse { Id = addressPersonId });
    }

    /// <summary>Update an address link.</summary>
    /// <param name="contactId">The Contact id.</param>
    /// <param name="addressPersonId">The address-link id (from the address list).</param>
    /// <param name="model">The updated address + link details and edit mode.</param>
    [HttpPut("{contactId:guid}/addresses/{addressPersonId:guid}")]
    [ValidateModel]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.EditContactDetails)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateAddressAsync([FromRoute] Guid contactId,
        [FromRoute] Guid addressPersonId, [FromBody] PersonAddressUpdateRequest model)
    {
        await contactService.UpdateAddressAsync(contactId, addressPersonId, model, CancellationToken);
        return Ok(new IdResponse { Id = addressPersonId });
    }

    /// <summary>Unlink an address from a contact.</summary>
    /// <param name="contactId">The Contact id.</param>
    /// <param name="addressPersonId">The address-link id to remove.</param>
    [HttpDelete("{contactId:guid}/addresses/{addressPersonId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.EditContactDetails)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RemoveAddressAsync([FromRoute] Guid contactId,
        [FromRoute] Guid addressPersonId)
    {
        await contactService.RemoveAddressAsync(contactId, addressPersonId, CancellationToken);
        return NoContent();
    }

    /// <summary>Add or replace the contact's photo. The image is resized server-side.</summary>
    /// <param name="contactId">The Contact id.</param>
    /// <param name="file">The uploaded image (multipart form field <c>file</c>).</param>
    [HttpPut("{contactId:guid}/photo")]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.EditContactDetails)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> SetPhotoAsync([FromRoute] Guid contactId, IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequestProblem("No file was uploaded.");
        }

        await using var stream = file.OpenReadStream();
        await contactService.SetPhotoAsync(contactId, stream, file.ContentType, file.FileName, CancellationToken);
        return Ok(new IdResponse { Id = contactId });
    }

    /// <summary>Stream the contact's photo. 404 if none.</summary>
    /// <param name="contactId">The Contact id.</param>
    [HttpGet("{contactId:guid}/photo")]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.ViewContactDetails)]
    public async Task<IActionResult> GetPhotoAsync([FromRoute] Guid contactId)
    {
        var content = await contactService.GetPhotoAsync(contactId, CancellationToken);

        var typedHeaders = Response.GetTypedHeaders();
        if (!string.IsNullOrWhiteSpace(content.Details.Hash))
        {
            typedHeaders.ETag = new Microsoft.Net.Http.Headers.EntityTagHeaderValue($"\"{content.Details.Hash}\"");
        }
        typedHeaders.LastModified = content.Details.LastModifiedAt;
        Response.Headers["X-Content-Type-Options"] = "nosniff";

        var safeContentType = SafeContentTypes.Sanitize(content.Details.ContentType);
        return File(content.Content, safeContentType, content.Details.FileName,
            enableRangeProcessing: content.Content.CanSeek);
    }

    /// <summary>Remove the contact's photo.</summary>
    /// <param name="contactId">The Contact id.</param>
    [HttpDelete("{contactId:guid}/photo")]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.EditContactDetails)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> DeletePhotoAsync([FromRoute] Guid contactId)
    {
        await contactService.DeletePhotoAsync(contactId, CancellationToken);
        return Ok(new IdResponse { Id = contactId });
    }

    /// <summary>Create a new contact (Person + Contact) from basic details. Returns the new Contact id.</summary>
    /// <param name="model">The basic-details payload.</param>
    [HttpPost]
    [ValidateModel]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.EditContactDetails)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateAsync([FromBody] ContactBasicDetailsUpsertRequest model)
    {
        var id = await contactService.CreateAsync(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Search existing People for the "new contact" flow, so someone already on file gets a
    /// contact role rather than a duplicate Person. Empty for blank/too-short queries.</summary>
    /// <param name="query">The search term.</param>
    [HttpGet("person-matches")]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.EditContactDetails)]
    [ProducesResponseType(typeof(IReadOnlyList<ContactMatchResponse>), 200)]
    public async Task<IActionResult> SearchPeopleAsync([FromQuery] string? query)
    {
        var result = await contactService.SearchPeopleAsync(query, CancellationToken);
        return Ok(result);
    }

    /// <summary>Attach a contact role to an existing Person. Returns the new Contact id.</summary>
    /// <param name="model">The person to attach a contact role to.</param>
    [HttpPost("for-person")]
    [ValidateModel]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.EditContactDetails)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateForPersonAsync([FromBody] ContactCreateForPersonRequest model)
    {
        var id = await contactService.CreateForPersonAsync(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Soft-delete a contact. The underlying Person is left intact.</summary>
    /// <param name="contactId">The Contact id.</param>
    [HttpDelete("{contactId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Contact.EditContactDetails)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid contactId)
    {
        await contactService.DeleteAsync(contactId, CancellationToken);
        return NoContent();
    }
}
