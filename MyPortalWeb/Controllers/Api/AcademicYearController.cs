﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Database.Enums;
using MyPortal.Logic.Attributes;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Requests.Curriculum;

namespace MyPortalWeb.Controllers.Api
{
    [Authorize]
    [Route("api/academicYears")]
    public sealed class AcademicYearController : ControllerBase
    {
        private readonly IAcademicYearService _academicYearService;

        public AcademicYearController(IAcademicYearService academicYearService)
        {
            _academicYearService = academicYearService;
        }

        [HttpPost]
        [Route("")]
        [Permission(PermissionValue.CurriculumAcademicStructure)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CreateAcademicYear([FromBody] AcademicYearRequestModel requestModel)
        {
            await _academicYearService.CreateAcademicYear(requestModel);

            return Ok();
        }
    }
}