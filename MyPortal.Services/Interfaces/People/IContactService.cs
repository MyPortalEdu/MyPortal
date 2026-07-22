using MyPortal.Contracts.Models.Documents;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Contacts;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.People;

public interface IContactService
{
    /// <summary>Paged contact summary for the contact list / picker.</summary>
    Task<PageResult<ContactSummaryResponse>> GetContactsAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);

    /// <summary>Contact profile header — identity + photo. 404 if not found.</summary>
    Task<ContactHeaderResponse> GetHeaderAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>The contact's associated students (read-only reverse view of the Family relationship).
    /// 404 if the contact doesn't exist.</summary>
    Task<IReadOnlyList<ContactStudentResponse>> GetAssociatedStudentsAsync(Guid id,
        CancellationToken cancellationToken);

    /// <summary>Basic-details area read (person bio + the Contact-row fields). 404 if not found.</summary>
    Task<ContactBasicDetailsResponse> GetBasicDetailsAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Basic-details area write — touches person bio and the Contact-row fields (parental
    /// ballot, place of work, job title, NI number).</summary>
    Task UpdateBasicDetailsAsync(Guid id, ContactBasicDetailsUpsertRequest model,
        CancellationToken cancellationToken);

    /// <summary>Contact-details area read (the contact's own emails + phone numbers). Part of the
    /// BasicDetails permission domain. 404 if not found.</summary>
    Task<PersonContactDetailsResponse> GetContactDetailsAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Contact-details area write — whole-collection replace of emails + phones.</summary>
    Task UpdateContactDetailsAsync(Guid id, PersonContactDetailsUpsertRequest model,
        CancellationToken cancellationToken);

    /// <summary>List the contact's linked addresses.</summary>
    Task<AddressListResponse> GetAddressesAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Search existing addresses to link (part of editing the area).</summary>
    Task<IReadOnlyList<AddressMatchResponse>> SearchAddressesAsync(string? query,
        CancellationToken cancellationToken);

    /// <summary>Link an existing address or create a new one. Returns the new link (AddressPerson) id.</summary>
    Task<Guid> AddAddressAsync(Guid id, PersonAddressUpsertRequest model, CancellationToken cancellationToken);

    /// <summary>Update an address link (or the shared address, per AddressEditMode).</summary>
    Task UpdateAddressAsync(Guid id, Guid addressPersonId, PersonAddressUpdateRequest model,
        CancellationToken cancellationToken);

    /// <summary>Unlink an address from the contact.</summary>
    Task RemoveAddressAsync(Guid id, Guid addressPersonId, CancellationToken cancellationToken);

    /// <summary>Adds or replaces the contact's photo (part of basic details). The image is resized
    /// before storage.</summary>
    Task SetPhotoAsync(Guid id, Stream image, string contentType, string fileName,
        CancellationToken cancellationToken);

    /// <summary>Opens the contact's photo for streaming; the caller disposes the content. 404 if the
    /// contact has no photo.</summary>
    Task<DocumentContentResponse> GetPhotoAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Removes the contact's photo.</summary>
    Task DeletePhotoAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates Person + Contact in one transaction with basic details only. Other areas are populated
    /// post-creation via their area PUTs. Returns the new Contact id.
    /// </summary>
    Task<Guid> CreateAsync(ContactBasicDetailsUpsertRequest model, CancellationToken cancellationToken);

    /// <summary>
    /// Search existing People for the create flow so someone already on file gets a contact role
    /// attached to their existing Person rather than a duplicate. Returns an empty list for blank or
    /// too-short queries rather than dumping the table.
    /// </summary>
    Task<IReadOnlyList<ContactMatchResponse>> SearchPeopleAsync(string? query, CancellationToken cancellationToken);

    /// <summary>
    /// Attach a contact role to an existing Person — creates only the Contact row hanging off the
    /// supplied person; no Person row is created and the person's bio is left untouched. 404 if the
    /// person doesn't exist; 400 if the person is already (active) a contact. Returns the new Contact id.
    /// </summary>
    Task<Guid> CreateForPersonAsync(ContactCreateForPersonRequest model, CancellationToken cancellationToken);

    /// <summary>Soft-deletes the Contact row; the underlying Person is left intact.</summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
