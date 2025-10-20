using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.System.Roles;
using MyPortal.Services.Interfaces.Services;
using MyPortal.WebApi.Infrastructure.Attributes;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

public class RolesController : BaseApiController<RolesController>
{
    private readonly IRoleService _roleService;

    public RolesController(ProblemDetailsFactory problemFactory, ILogger<RolesController> logger,
        IRoleService roleService) : base(problemFactory, logger)
    {
        _roleService = roleService;
    }

    [HttpGet("{roleId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.System.ViewRoles)]
    public async Task<IActionResult> GetRoleDetailsAsync([FromRoute] Guid roleId)
    {
        var result = await _roleService.GetDetailsByIdAsync(roleId, CancellationToken);

        if (result == null)
        {
            return NotFoundProblem("Role not found.");
        }
        
        return Ok(result);
    }

    [HttpGet]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.System.ViewRoles)]
    public async Task<IActionResult> GetRolesAsync(FilterOptions? filter, SortOptions? sort, PageOptions? paging)
    {
        var result = await _roleService.GetRolesAsync(filter, sort, paging, CancellationToken);

        return Ok(result);
    }

    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.System.EditRoles)]
    public async Task<IActionResult> CreateRoleAsync([FromBody] RoleUpsertDto model)
    {
        var result = await _roleService.CreateRoleAsync(model, CancellationToken);

        return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
    }

    [HttpPut("{roleId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.System.EditRoles)]
    public async Task<IActionResult> UpdateRoleAsync([FromRoute] Guid roleId, [FromBody] RoleUpsertDto model)
    {
        var result = await _roleService.UpdateRoleAsync(roleId, model, CancellationToken);

        return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
    }

    [HttpDelete("{roleId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.System.EditRoles)]
    public async Task<IActionResult> DeleteRoleAsync([FromRoute] Guid roleId)
    {
        var result = await _roleService.DeleteRoleAsync(roleId, CancellationToken);
        
        return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
    }
}