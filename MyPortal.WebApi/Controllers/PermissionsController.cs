using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;

namespace MyPortal.WebApi.Controllers
{
    public class PermissionsController : BaseApiController<PermissionsController>
    {
        private readonly IPermissionService _permissionService;

        public PermissionsController(ProblemDetailsFactory problemFactory, ILogger<PermissionsController> logger,
            IPermissionService permissionService) : base(problemFactory, logger)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        [UserType(UserType.Staff)]
        [Permission(PermissionMode.RequireAny, Permissions.System.ViewRoles, Permissions.System.EditRoles)]
        public async Task<IActionResult> GetPermissionsAsync()
        {
            var result = await _permissionService.GetAllPermissionsAsync(CancellationToken);

            return Ok(result);
        }
    }
}
