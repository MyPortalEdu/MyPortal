using System.Collections.Generic;
using System.Threading.Tasks;
using MyPortal.Database.Models.Entity;
using MyPortal.Database.Models.Search;

namespace MyPortal.Database.Interfaces.Repositories
{
    public interface IAgencyRepository : IReadWriteRepository<Agency>, IUpdateRepository<Agency>
    {
        Task<IEnumerable<Agency>> GetAll(AgencySearchOptions searchOptions);
    }
}