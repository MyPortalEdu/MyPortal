using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.System.Permissions;

namespace MyPortal.WebApi.Controllers
{
    /// <summary>Permission catalogue endpoints.</summary>
    public sealed class PermissionsController(
        ProblemDetailsFactory problemFactory,
        ILogger<PermissionsController> logger,
        IPermissionService permissionService)
        : BaseApiController(problemFactory, logger)
    {
        /// <summary>List every permission seeded into the database.</summary>
        /// <remarks>Returned grouped by area for tree-style UIs.</remarks>
        [HttpGet]
        [UserType(UserType.Staff)]
        [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.ViewRoles, Permissions.SystemAdmin.EditRoles)]
        [ProducesResponseType(typeof(IList<PermissionResponse>), 200)]
        public async Task<IActionResult> GetPermissionsAsync([FromQuery] UserType? userType = null)
        {
            var result = await permissionService.GetAllPermissionsAsync(userType, CancellationToken);

            return Ok(result);
        }
    }
}
