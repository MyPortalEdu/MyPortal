using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MyPortal.Database.Models.Search;
using MyPortal.Logic.Models.Data.Agents;
using MyPortal.Logic.Models.Requests.Agents;

namespace MyPortal.Logic.Interfaces.Services;

public interface IAgencyService : IService
{
    Task<IEnumerable<AgencyModel>> GetAgencies(AgencySearchOptions searchOptions);
    Task<AgencyModel> GetAgencyById(Guid agencyId);
    Task<AgencyModel> CreateAgency(AgencyRequestModel agency);
    Task<AgencyModel> UpdateAgency(Guid agencyId, AgencyRequestModel agency);
    Task<bool> DeleteAgency(Guid agencyId);
}