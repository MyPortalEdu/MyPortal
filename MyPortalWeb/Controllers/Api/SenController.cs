using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Database.Enums;
using MyPortal.Logic.Attributes;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.Students;

namespace MyPortalWeb.Controllers.Api
{
    public class SenController : ControllerBase
    {
        private readonly ISenService _senService;

        public SenController(ISenService senService)
        {
            _senService = senService;
        }

        [HttpGet]
        [Route("api/students/{studentId}/send/giftedTalented")]
        [Permission(PermissionValue.StudentViewSenDetails)]
        [ProducesResponseType(typeof(IEnumerable<GiftedTalentedModel>), 200)]
        public async Task<IActionResult> GetGiftedTalentedByStudent([FromRoute] Guid studentId)
        {
            var giftedTalented = await _senService.GetGiftedTalentedSubjects(studentId);

            return Ok(giftedTalented);
        }
    }
}