using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Database.Models.Entity;
using MyPortal.Database.Models.Search;
using MyPortal.Logic.Interfaces;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.Agents;
using MyPortal.Logic.Models.Requests.Agents;

namespace MyPortal.Logic.Services;

public class AgencyService : BaseService, IAgencyService
{
    public AgencyService(ISessionUser user) : base(user)
    {
    }

    public async Task<IEnumerable<AgencyModel>> GetAgencies(AgencySearchOptions searchOptions)
    {
        await using var unitOfWork = await User.GetConnection();

        var agencies = await unitOfWork.GetRepository<IAgencyRepository>().GetAll(searchOptions);
        
        return agencies.Select(a => new AgencyModel(a)).ToList();
    }

    public async Task<AgencyModel> GetAgencyById(Guid agencyId)
    {
        await using var unitOfWork = await User.GetConnection();

        var agency = await unitOfWork.GetRepository<IAgencyRepository>().GetById(agencyId);

        return new AgencyModel(agency);
    }

    public async Task<AgencyModel> CreateAgency(AgencyRequestModel model)
    {
        Validate(model);
        
        await using var unitOfWork = await User.GetConnection();

        var agency = new Agency
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Website = model.Website,
            TypeId = model.AgencyTypeId,
        };
        
        unitOfWork.GetRepository<IAgencyRepository>().Create(agency);
        await unitOfWork.SaveChangesAsync();
        
        return new AgencyModel(agency);
    }

    public async Task<AgencyModel> UpdateAgency(Guid agencyId, AgencyRequestModel model)
    {
        Validate(model);
        
        await using var unitOfWork = await User.GetConnection();
        
        var agency = await unitOfWork.GetRepository<IAgencyRepository>().GetById(agencyId);

        agency.Name = model.Name;
        agency.Website = model.Website;
        agency.TypeId = model.AgencyTypeId;
        
        await unitOfWork.GetRepository<IAgencyRepository>().Update(agency);
        
        await unitOfWork.SaveChangesAsync();
        
        return new AgencyModel(agency);
    }

    public async Task<bool> DeleteAgency(Guid agencyId)
    {
        await using var unitOfWork = await User.GetConnection();

        await unitOfWork.GetRepository<IAgencyRepository>().Delete(agencyId);

        return true;
    }
}