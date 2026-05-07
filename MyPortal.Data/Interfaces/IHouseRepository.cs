using MyPortal.Contracts.Models.Pastoral;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Interfaces;

public interface IHouseRepository : IEntityRepository<House>
{
    Task<IList<House>> GetHousesByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);

    Task<PageResult<HouseSummaryResponse>> GetSummariesAsync(Guid academicYearId,
        FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);

    // Returns null when the house doesn't exist (caller maps to 404). Internal carrier
    // bundles the header + supervisors so the service can flatten into HouseDetailsResponse.
    Task<HouseDetailsResult?> GetDetailsByIdAsync(Guid houseId, CancellationToken cancellationToken);
}

public class HouseDetailsResult
{
    public HouseDetailsResponse Header { get; set; } = null!;
    public IList<StudentGroupSupervisorResponse> Supervisors { get; set; } = new List<StudentGroupSupervisorResponse>();
}

