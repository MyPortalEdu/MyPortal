using MyPortal.Contracts.Models.Pastoral;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Interfaces;

public interface IRegGroupRepository : IEntityRepository<RegGroup>
{
    Task<IList<RegGroup>> GetRegGroupsByAcademicYear(Guid academicYearId,
        CancellationToken cancellationToken);

    Task<PageResult<RegGroupSummaryResponse>> GetSummariesAsync(Guid academicYearId,
        FilterOptions? filter = null, SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);
    
    Task<RegGroupDetailsResponse?> GetDetailsByIdAsync(Guid regGroupId,
        CancellationToken cancellationToken);
}
