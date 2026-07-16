using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.System.Roles;
using MyPortal.Services.Interfaces.System;
using MyPortal.WebApi.Infrastructure.Attributes;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

/// <summary>Role management endpoints.</summary>
public sealed class RolesController : BaseApiController
{
    private readonly IRoleService _roleService;

    public RolesController(ProblemDetailsFactory problemFactory, ILogger<RolesController> logger,
        IRoleService roleService) : base(problemFactory, logger)
    {
        _roleService = roleService;
    }

    /// <summary>Get a role, including its permissions.</summary>
    /// <param name="roleId">The id of the role.</param>
    [HttpGet("{roleId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.ViewRoles)]
    [ProducesResponseType(typeof(RoleDetailsResponse), 200)]
    public async Task<IActionResult> GetRoleDetailsAsync([FromRoute] Guid roleId)
    {
        var result = await _roleService.GetDetailsByIdAsync(roleId, CancellationToken);

        if (result == null)
        {
            return NotFoundProblem("Role not found.");
        }

        return Ok(result);
    }

    /// <summary>Page through role summaries.</summary>
    /// <remarks>Supports server-side filtering, sorting, and paging. Page size is clamped.</remarks>
    [HttpGet]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.ViewRoles)]
    [ProducesResponseType(typeof(PageResult<RoleSummaryResponse>), 200)]
    public async Task<IActionResult> GetRolesAsync([FromQuery] int page, [FromQuery] int pageSize,
        [FromQuery] FilterOptions? filter, [FromQuery] SortOptions? sort)
    {
        var options = GetListingOptions(page, pageSize, filter, sort);

        var result = await _roleService.GetRolesAsync(options.FilterOptions, options.SortOptions, options.PageOptions);

        return Ok(result);
    }

    /// <summary>Create a role with an initial permission set.</summary>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.EditRoles)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> CreateRoleAsync([FromBody] RoleUpsertRequest model)
    {
        var result = await _roleService.CreateAsync(model, CancellationToken);

        return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
    }

    /// <summary>Update a role's name and/or permission set.</summary>
    /// <remarks>Permission changes propagate after the security-stamp validation interval.</remarks>
    /// <param name="roleId">The id of the role to update.</param>
    /// <param name="model">The updated name and permissions.</param>
    [HttpPut("{roleId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.EditRoles)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> UpdateRoleAsync([FromRoute] Guid roleId, [FromBody] RoleUpsertRequest model)
    {
        var result = await _roleService.UpdateAsync(roleId, model, CancellationToken);

        return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
    }

    /// <summary>Delete a role.</summary>
    /// <remarks>System roles cannot be deleted.</remarks>
    /// <param name="roleId">The id of the role to delete.</param>
    [HttpDelete("{roleId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.EditRoles)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteRoleAsync([FromRoute] Guid roleId)
    {
        var result = await _roleService.DeleteAsync(roleId, CancellationToken);

        return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
    }
}
