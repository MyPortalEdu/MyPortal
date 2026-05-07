using MyPortal.Contracts.Models.Pastoral;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Interfaces;

public interface IYearGroupRepository : IEntityRepository<YearGroup>
{
    Task<IList<YearGroup>> GetYearGroupsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);

    Task<PageResult<YearGroupSummaryResponse>> GetSummariesAsync(Guid academicYearId,
        FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);

    // Returns null when the year group doesn't exist (caller maps to 404). Internal
    // carrier bundles the header + supervisors so the service can flatten into
    // YearGroupDetailsResponse.
    Task<YearGroupDetailsResult?> GetDetailsByIdAsync(Guid yearGroupId,
        CancellationToken cancellationToken);
}

public class YearGroupDetailsResult
{
    public YearGroupDetailsResponse Header { get; set; } = null!;
    public IList<YearGroupSupervisorResponse> Supervisors { get; set; } = new List<YearGroupSupervisorResponse>();
}
