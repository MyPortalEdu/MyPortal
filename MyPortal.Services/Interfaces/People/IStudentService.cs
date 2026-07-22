using MyPortal.Contracts.Models.Documents;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Students;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.People;

public interface IStudentService
{
    /// <summary>Paged student summary for the student list / picker.</summary>
    Task<PageResult<StudentSummaryResponse>> GetStudentsAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);

    /// <summary>Student profile header — identity + admission number + status. 404 if not found.</summary>
    Task<StudentHeaderResponse> GetHeaderAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Basic-details area read (person bio + admission number). 404 if not found.</summary>
    Task<StudentBasicDetailsResponse> GetBasicDetailsAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Registration area read — enrolment/boarder status, admission date, UPN/ULN, part-time,
    /// plus the picker option lists. 404 if not found.</summary>
    Task<StudentRegistrationDetailsResponse> GetRegistrationDetailsAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Registration area write — touches the enrolment / statutory-identifier fields on the
    /// student record only.</summary>
    Task UpdateRegistrationDetailsAsync(Guid id, StudentRegistrationDetailsUpsertRequest model,
        CancellationToken cancellationToken);

    /// <summary>Generate a suggested permanent UPN from the local school's LA code, establishment
    /// number, the current year and the next available serial. Not persisted — the caller writes it
    /// into the UPN field. Fails (400) if the school's LA / establishment number aren't configured.</summary>
    Task<GeneratedUpnResponse> GenerateUpnAsync(CancellationToken cancellationToken);

    /// <summary>Basic-details area write — touches person bio only (admission number is immutable,
    /// cultural/medical/registration fields are owned by their own areas).</summary>
    Task UpdateBasicDetailsAsync(Guid id, StudentBasicDetailsUpsertRequest model,
        CancellationToken cancellationToken);

    /// <summary>Contact-details area read (the student's own emails + phone numbers). Part of the
    /// BasicDetails permission domain. 404 if not found.</summary>
    Task<PersonContactDetailsResponse> GetContactDetailsAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Contact-details area write — whole-collection replace of emails + phones.</summary>
    Task UpdateContactDetailsAsync(Guid id, PersonContactDetailsUpsertRequest model,
        CancellationToken cancellationToken);

    /// <summary>List the student's linked addresses.</summary>
    Task<AddressListResponse> GetAddressesAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Search existing addresses to link (part of editing the area).</summary>
    Task<IReadOnlyList<AddressMatchResponse>> SearchAddressesAsync(string? query,
        CancellationToken cancellationToken);

    /// <summary>Link an existing address or create a new one. Returns the new link (AddressPerson) id.</summary>
    Task<Guid> AddAddressAsync(Guid id, PersonAddressUpsertRequest model, CancellationToken cancellationToken);

    /// <summary>Update an address link (or the shared address, per AddressEditMode).</summary>
    Task UpdateAddressAsync(Guid id, Guid addressPersonId, PersonAddressUpdateRequest model,
        CancellationToken cancellationToken);

    /// <summary>Unlink an address from the student.</summary>
    Task RemoveAddressAsync(Guid id, Guid addressPersonId, CancellationToken cancellationToken);

    /// <summary>Adds or replaces the student's photo (part of basic details). The image is resized
    /// before storage.</summary>
    Task SetPhotoAsync(Guid id, Stream image, string contentType, string fileName,
        CancellationToken cancellationToken);

    /// <summary>Opens the student's photo for streaming; the caller disposes the content. 404 if the
    /// student has no photo.</summary>
    Task<DocumentContentResponse> GetPhotoAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Removes the student's photo.</summary>
    Task DeletePhotoAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates Person + Student in one transaction with basic details only. The admission number is
    /// auto-assigned. Registration, cultural, medical and welfare areas are populated post-creation
    /// via their area PUTs. Returns the new Student id.
    /// </summary>
    Task<Guid> CreateAsync(StudentBasicDetailsUpsertRequest model, CancellationToken cancellationToken);

    /// <summary>
    /// Search existing People for the create flow so someone already on file gets a student role
    /// attached to their existing Person rather than a duplicate. Returns an empty list for blank or
    /// too-short queries rather than dumping the table.
    /// </summary>
    Task<IReadOnlyList<StudentMatchResponse>> SearchPeopleAsync(string? query, CancellationToken cancellationToken);

    /// <summary>
    /// Attach a student role to an existing Person — creates only the Student row (with an
    /// auto-assigned admission number) hanging off the supplied person; no Person row is created and
    /// the person's bio is left untouched. 404 if the person doesn't exist; 400 if the person is
    /// already (active) a student. Returns the new Student id.
    /// </summary>
    Task<Guid> CreateForPersonAsync(StudentCreateForPersonRequest model, CancellationToken cancellationToken);

    /// <summary>Soft-deletes the Student row; the underlying Person is left intact.</summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
