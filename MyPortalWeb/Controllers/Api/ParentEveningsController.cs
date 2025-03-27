using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Logic.Interfaces.Services;

namespace MyPortalWeb.Controllers.Api
{
    [Microsoft.AspNetCore.Components.Route("api/parentEvenings")]
    public class ParentEveningsController : ControllerBase
    {
        private readonly IParentEveningService _parentEveningService;

        public ParentEveningsController(IParentEveningService parentEveningService)
        {
            _parentEveningService = parentEveningService;
        }

        [HttpGet]
        [Route("templates/{parentEveningId}/{staffMemberId}")]
        public async Task<IActionResult> GetParentEveningTemplatesByStaffMember([FromRoute] Guid parentEveningId,
            [FromRoute] Guid staffMemberId)
        {
            var parentEveningTemplates =
                await _parentEveningService.GetAppointmentTemplatesByStaffMember(parentEveningId, staffMemberId);

            return Ok(parentEveningTemplates);
        }
    }
}