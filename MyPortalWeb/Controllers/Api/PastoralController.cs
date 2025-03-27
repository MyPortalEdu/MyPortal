using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.Curriculum;

namespace MyPortalWeb.Controllers.Api
{
    [Authorize]
    [Route("api/pastoral")]
    public class PastoralController : ControllerBase
    {
        private readonly IPastoralService _pastoralService;

        public PastoralController(IPastoralService pastoralService)
        {
            _pastoralService = pastoralService;
        }

        [HttpGet]
        [Route("houses")]
        [ProducesResponseType(typeof(IEnumerable<HouseModel>), 200)]
        public async Task<IActionResult> GetHouses()
        {
            var houses = await _pastoralService.GetHouses();

            return Ok(houses);
        }

        [HttpGet]
        [Route("regGroups")]
        [ProducesResponseType(typeof(IEnumerable<RegGroupModel>), 200)]
        public async Task<IActionResult> GetRegGroups()
        {
            var regGroups = await _pastoralService.GetRegGroups();

            return Ok(regGroups);
        }

        [HttpGet]
        [Route("yearGroups")]
        [ProducesResponseType(typeof(IEnumerable<YearGroupModel>), 200)]
        public async Task<IActionResult> GetYearGroups()
        {
            var yearGroups = await _pastoralService.GetYearGroups();

            return Ok(yearGroups);
        }
    }
}