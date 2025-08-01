using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Logic.Interfaces.Services;
using MyPortalWeb.Models.Response;

namespace MyPortalWeb.Controllers.Api
{
    [Route("api/schools")]
    public class SchoolsController : ControllerBase
    {
        private readonly ISchoolService _schoolService;

        public SchoolsController(ISchoolService schoolService)
        {
            _schoolService = schoolService;
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("local/name")]
        [ProducesResponseType(typeof(string), 200)]
        public async Task<IActionResult> GetLocalSchoolName()
        {
            var schoolName = await _schoolService.GetLocalSchoolName();

            return Ok(new StringResponseModel(schoolName));
        }
    }
}