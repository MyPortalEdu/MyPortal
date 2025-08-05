using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Database.Enums;
using MyPortal.Database.Models.Search;
using MyPortal.Logic.Attributes;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.Agents;
using MyPortal.Logic.Models.Requests.Agents;

namespace MyPortalWeb.Controllers.Api;

[Authorize]
[Route("api/agencies")]
public class AgenciesController : ControllerBase
{
    private readonly IAgencyService _agencyService;

    public AgenciesController(IAgencyService agencyService)
    {
        _agencyService = agencyService;
    }
    
    [HttpGet]
    [Route("")]
    [Permission(PermissionValue.AgencyViewAgencies)]
    [ProducesResponseType(typeof(IEnumerable<AgencyModel>), 200)]
    public async Task<IActionResult> GetAgencies([FromBody] AgencySearchOptions searchOptions)
    {
        var agencies = (await _agencyService.GetAgencies(searchOptions)).ToList();

        return Ok(agencies);
    }

    [HttpPost]
    [Route("")]
    [Permission(PermissionValue.AgencyEditAgencies)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> CreateAgency([FromBody] AgencyRequestModel agencyModel)
    {
        await _agencyService.CreateAgency(agencyModel);
        
        return Ok();
    }

    [HttpPut]
    [Route("{agencyId}")]
    [Permission(PermissionValue.AgencyEditAgencies)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> UpdateAgency([FromRoute] Guid agencyId, [FromBody] AgencyRequestModel agencyModel)
    {
        await _agencyService.UpdateAgency(agencyId, agencyModel);

        return Ok();
    }

    [HttpDelete]
    [Route("{agencyId}")]
    [Permission(PermissionValue.AgencyEditAgencies)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> DeleteAgency([FromRoute] Guid agencyId)
    {
        await _agencyService.DeleteAgency(agencyId);

        return Ok();
    }
}