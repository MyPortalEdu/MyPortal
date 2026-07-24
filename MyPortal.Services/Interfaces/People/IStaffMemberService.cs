using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Contracts.Models.People;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.People;

public interface IStaffMemberService
{
    /// <summary>Paged staff summary for the staff/head-teacher picker.</summary>
    Task<PageResult<StaffMemberSummaryResponse>> GetStaffMembersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);

    /// <summary>Staff profile header — identity + status + viewer relationship.
    /// 403 if no view-basic-details scope covers the subject.</summary>
    Task<StaffMemberHeaderResponse> GetHeaderAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Basic-details area read (person bio + Code). Gated via the resolver.</summary>
    Task<StaffBasicDetailsResponse> GetBasicDetailsAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Basic-details area write — touches person bio (sans equality fields) and the staff
    /// code only. Gated via the resolver.</summary>
    Task UpdateBasicDetailsAsync(Guid id, StaffBasicDetailsUpsertRequest model,
        CancellationToken cancellationToken);

    Task<StaffManagementResponse> GetManagementAsync(Guid id, CancellationToken cancellationToken);

    Task SetLineManagerAsync(Guid id, SetStaffLineManagerRequest model, CancellationToken cancellationToken);

    /// <summary>Adds or replaces the staff member's photo (part of basic details). Gated on
    /// basic-details edit access. The image is resized before storage.</summary>
    Task SetPhotoAsync(Guid id, Stream image, string contentType, string fileName,
        CancellationToken cancellationToken);

    /// <summary>Opens the staff member's photo for streaming; the caller disposes the content.
    /// Gated on basic-details view access. 404 if the member has no photo.</summary>
    Task<DocumentContentResponse> GetPhotoAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Removes the staff member's photo. Gated on basic-details edit access.</summary>
    Task DeletePhotoAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates Person + StaffMember in one transaction with basic details only — the joiner
    /// data HR knows at create time (name, code, DOB, gender). Other areas (equality,
    /// employment, professional, etc.) are populated post-creation via their area PUTs.
    /// Returns the new StaffMember id.
    /// </summary>
    Task<Guid> CreateAsync(StaffBasicDetailsUpsertRequest model, CancellationToken cancellationToken);

    /// <summary>
    /// Search existing People for the create flow so a joiner already on file (as a
    /// contact/agent/former student/...) gets a staff role attached to their existing Person
    /// rather than a duplicate. Gated on EditAll (the create capability). Returns an empty list
    /// for blank or too-short queries rather than dumping the table.
    /// </summary>
    Task<IReadOnlyList<PersonMatchResponse>> SearchPeopleAsync(string? query, CancellationToken cancellationToken);

    /// <summary>
    /// Attach a staff role to an existing Person — creates only the StaffMember row (with its
    /// Code) hanging off the supplied person; no Person row is created and the person's bio is
    /// left untouched. 404 if the person doesn't exist; 400 if the person is already (active)
    /// staff. Returns the new StaffMember id.
    /// </summary>
    Task<Guid> CreateForPersonAsync(StaffMemberCreateForPersonRequest model, CancellationToken cancellationToken);

    /// <summary>
    /// Advisory availability check for a staff code — true when the code is not already in use.
    /// Blank codes are reported available (nothing to clash with). Pass <paramref name="excludeStaffMemberId"/>
    /// when editing so a record does not clash with itself. This is a convenience check for inline
    /// UI feedback; the authoritative guard still runs on create/update.
    /// </summary>
    Task<bool> IsCodeAvailableAsync(string? code, Guid? excludeStaffMemberId,
        CancellationToken cancellationToken);

    /// <summary>Soft-deletes the StaffMember row; the underlying Person is left intact.</summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
