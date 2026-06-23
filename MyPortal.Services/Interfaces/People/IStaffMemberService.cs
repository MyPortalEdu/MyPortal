using MyPortal.Common.Interfaces;
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

    /// <summary>Soft-deletes the StaffMember row; the underlying Person is left intact.</summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
