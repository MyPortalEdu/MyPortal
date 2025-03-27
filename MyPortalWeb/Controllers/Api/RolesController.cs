using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Database.Enums;
using MyPortal.Logic.Attributes;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.Settings;
using MyPortal.Logic.Models.Requests.Settings.Roles;
using MyPortal.Logic.Models.Structures;
using MyPortalWeb.Models.Response;

namespace MyPortalWeb.Controllers.Api
{
    [Authorize]
    [Route("api/roles")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost]
        [Route("")]
        [Permission(PermissionValue.SystemEditGroups)]
        [ProducesResponseType(typeof(NewEntityResponseModel), 200)]
        public async Task<IActionResult> CreateRole([FromBody] RoleRequestModel model)
        {
            var newId = (await _roleService.CreateRole(model)).FirstOrDefault();

            return Ok(new NewEntityResponseModel { Id = newId });
        }

        [HttpPut]
        [Route("{roleId}")]
        [Permission(PermissionValue.SystemEditGroups)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> UpdateRole([FromRoute] Guid roleId, [FromBody] RoleRequestModel model)
        {
            await _roleService.UpdateRole(roleId, model);

            return Ok();
        }

        [HttpDelete]
        [Route("{roleId}")]
        [Permission(PermissionValue.SystemEditGroups)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> DeleteRole([FromRoute] Guid roleId)
        {
            await _roleService.DeleteRole(roleId);

            return Ok();
        }

        [HttpGet]
        [Route("")]
        [Permission(PermissionValue.SystemViewGroups)]
        [ProducesResponseType(typeof(IEnumerable<RoleModel>), 200)]
        public async Task<IActionResult> GetRoles([FromQuery] string roleName)
        {
            IEnumerable<RoleModel> roles = await _roleService.GetRoles(roleName);
            
                            return Ok(roles);
        }

        [HttpGet]
        [Route("{roleId}")]
        [Permission(PermissionValue.SystemViewGroups)]
        [ProducesResponseType(typeof(RoleModel), 200)]
        public async Task<IActionResult> GetRoleById([FromRoute] Guid roleId)
        {
            // Don't get cached version - get from database
            var role = await _roleService.GetRoleById(roleId, false);

            return Ok(role);
        }

        [HttpGet]
        [Route("{roleId}/permissions")]
        [Permission(PermissionValue.SystemViewGroups)]
        [ProducesResponseType(typeof(TreeNode), 200)]
        public async Task<IActionResult> GetPermissionsTree([FromRoute] Guid roleId)
        {
            var permissionsTree = await _roleService.GetPermissionsTree(roleId);

            return Ok(permissionsTree);
        }
    }
}