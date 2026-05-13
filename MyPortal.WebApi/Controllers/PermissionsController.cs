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
    /// <summary>
    /// Read-only catalogue of permissions defined by the application. Used by the
    /// role-editor UI to render the permission tree when granting/revoking on a role.
    /// </summary>
    public sealed class PermissionsController : BaseApiController
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(ProblemDetailsFactory problemFactory, ILogger<PermissionsController> logger,
            IPermissionService permissionService) : base(problemFactory, logger)
        {
            _permissionService = permissionService;
        }

        /// <summary>List every permission seeded into the database.</summary>
        /// <remarks>
        /// Permissions are code-defined constants (see
        /// <c>MyPortal.Auth.Constants.Permissions</c>) and are seeded into the DB on
        /// boot. Returned grouped by area so the UI can render a tree.
        /// </remarks>
        [HttpGet]
        [UserType(UserType.Staff)]
        [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.ViewRoles, Permissions.SystemAdmin.EditRoles)]
        [ProducesResponseType(typeof(IList<PermissionResponse>), 200)]
        public async Task<IActionResult> GetPermissionsAsync()
        {
            var result = await _permissionService.GetAllPermissionsAsync(CancellationToken);

            return Ok(result);
        }
    }
}
