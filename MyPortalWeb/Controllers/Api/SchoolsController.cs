﻿using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Database.Constants;
using MyPortal.Database.Enums;
using MyPortal.Database.Models.Filters;
using MyPortal.Database.Models.Search;
using MyPortal.Logic.Attributes;
using MyPortal.Logic.Constants;
using MyPortal.Logic.Extensions;
using MyPortal.Logic.Identity;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.School;
using MyPortal.Logic.Models.Requests.School.Bulletins;
using MyPortal.Logic.Services;
using MyPortalWeb.Controllers.BaseControllers;
using MyPortalWeb.Models.Response;

namespace MyPortalWeb.Controllers.Api
{
    [Route("api/schools")]
    public class SchoolsController : BaseApiController
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
            try
            {
                var schoolName = await _schoolService.GetLocalSchoolName();

                return Ok(new StringResponseModel(schoolName));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        [HttpGet]
        [Route("local/bulletins")]
        [ProducesResponseType(typeof(BulletinPageResponse), 200)]
        public async Task<IActionResult> GetSchoolBulletins([FromQuery] BulletinSearchOptions searchOptions,
            [FromQuery] PageFilter filter)
        {
            try
            {
                var bulletins = await _schoolService.GetBulletinSummaries(searchOptions, filter);

                return Ok(bulletins);
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        [HttpPost]
        [Route("local/bulletins")]
        [Authorize(Policies.UserType.Staff)]
        [Permission(PermissionValue.SchoolEditSchoolBulletins)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> CreateBulletin([FromBody] BulletinRequestModel model)
        {
            try
            {
                await _schoolService.CreateBulletin(model);
                return Ok();
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        [HttpPost]
        [Route("local/bulletins/{bulletinId}")]
        [Authorize(Policies.UserType.Staff)]
        [Permission(PermissionValue.SchoolEditSchoolBulletins)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UpdateBulletin([FromRoute] Guid bulletinId,
            [FromBody] BulletinRequestModel model)
        {
            try
            {
                await _schoolService.UpdateBulletin(bulletinId, model);
                return Ok();
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        [HttpPost]
        [Route("local/bulletins/{bulletinId}/approve")]
        [Authorize(Policies.UserType.Staff)]
        [Permission(PermissionValue.SchoolApproveSchoolBulletins)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ApproveBulletin([FromRoute] Guid bulletinId,
            [FromBody] ApproveBulletinRequestModel model)
        {
            try
            {
                model.BulletinId = bulletinId;
                await _schoolService.SetBulletinApproved(model);
                return Ok();
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        [HttpDelete]
        [Route("local/bulletins/{bulletinId}")]
        [Authorize(Policies.UserType.Staff)]
        [Permission(PermissionValue.SchoolEditSchoolBulletins)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteBulletin([FromRoute] Guid bulletinId)
        {
            try
            {
                await _schoolService.DeleteBulletin(bulletinId);
                return Ok();
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }
    }
}