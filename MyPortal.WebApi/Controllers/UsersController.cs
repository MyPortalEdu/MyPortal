using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.System.Permissions;
using MyPortal.Contracts.Models.System.Users;
using MyPortal.Services.Interfaces.System;
using MyPortal.WebApi.Infrastructure.Attributes;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

/// <summary>User management endpoints.</summary>
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(ProblemDetailsFactory problemFactory, ILogger<UsersController> logger,
        IUserService userService) : base(problemFactory, logger)
    {
        _userService = userService;
    }

    /// <summary>Get a user by id.</summary>
    /// <remarks>Returns 404 if no user matches.</remarks>
    /// <param name="userId">The id of the user.</param>
    [HttpGet("{userId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.ViewUsers)]
    [ProducesResponseType(typeof(UserDetailsResponse), 200)]
    public async Task<IActionResult> GetUserDetailsByIdAsync([FromRoute] Guid userId)
    {
        var result = await _userService.GetDetailsByIdAsync(userId, CancellationToken);

        if (result == null)
        {
            return NotFoundProblem("User not found.");
        }

        return Ok(result);
    }

    /// <summary>Get a user's effective permissions — the union across their assigned roles.</summary>
    /// <param name="userId">The id of the user.</param>
    [HttpGet("{userId:guid}/permissions")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.ViewUsers)]
    [ProducesResponseType(typeof(IList<PermissionResponse>), 200)]
    public async Task<IActionResult> GetUserPermissionsAsync([FromRoute] Guid userId)
    {
        var result = await _userService.GetEffectivePermissionsAsync(userId, CancellationToken);

        return Ok(result);
    }

    /// <summary>Page through user summaries.</summary>
    /// <remarks>Supports server-side filtering, sorting, and paging. Page size is clamped.</remarks>
    /// <param name="page">1-based page number.</param>
    /// <param name="pageSize">Items per page (clamped 1..100).</param>
    /// <param name="filter">Optional QueryKit filter (Base64-encoded JSON).</param>
    /// <param name="sort">Optional QueryKit sort (Base64-encoded JSON).</param>
    [HttpGet]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.ViewUsers)]
    [ProducesResponseType(typeof(PageResult<UserSummaryResponse>), 200)]
    public async Task<IActionResult> GetUsersAsync([FromQuery] int page, [FromQuery] int pageSize,
        [FromQuery] FilterOptions? filter, [FromQuery] SortOptions? sort)
    {
        var options = GetListingOptions(page, pageSize, filter, sort);

        var result = await _userService.GetUsersAsync(options.FilterOptions, options.SortOptions, options.PageOptions,
            CancellationToken);

        return Ok(result);
    }

    /// <summary>Create a new user account.</summary>
    /// <remarks>Duplicate usernames/emails return <c>409</c>; other validation issues return <c>400</c>.</remarks>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.EditUsers)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateUserAsync([FromBody] UserUpsertRequest model)
    {
        var (result, userId) = await _userService.CreateAsync(model, CancellationToken);

        return !result.Succeeded ? IdentityResultProblem(result) : Ok(new IdResponse { Id = userId });
    }

    /// <summary>Update a user's metadata.</summary>
    /// <remarks>Does not change the password.</remarks>
    /// <param name="userId">The id of the user to update.</param>
    /// <param name="model">The updated metadata.</param>
    [HttpPut("{userId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.EditUsers)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> UpdateUserAsync([FromRoute] Guid userId, [FromBody] UserUpdateRequest model)
    {
        var result = await _userService.UpdateAsync(userId, model, CancellationToken);

        return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
    }

    /// <summary>Force-set a user's password.</summary>
    /// <remarks>Bypasses the existing password and updates the security stamp.</remarks>
    /// <param name="userId">The id of the user.</param>
    /// <param name="model">The new password.</param>
    [HttpPut("{userId:guid}/password")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.EditUsers)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> SetPasswordAsync([FromRoute] Guid userId, [FromBody] UserSetPasswordRequest model)
    {
        var result = await _userService.SetPasswordAsync(userId, model, CancellationToken);

        return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
    }

    /// <summary>Delete a user account.</summary>
    /// <remarks>System users cannot be deleted.</remarks>
    /// <param name="userId">The id of the user to delete.</param>
    [HttpDelete("{userId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.EditUsers)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteUserAsync([FromRoute] Guid userId)
    {
        var result = await _userService.DeleteAsync(userId, CancellationToken);

        return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
    }
}
