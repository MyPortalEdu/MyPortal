using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.System.Users;
using MyPortal.Services.Interfaces.System;
using MyPortal.WebApi.Infrastructure.Attributes;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// System-administration endpoints for managing user accounts. The current user's
/// own profile and password are handled via <see cref="MeController"/>; this
/// controller is for admins managing other users.
/// </summary>
public class UsersController : BaseApiController<UsersController>
{
    private readonly IUserService _userService;

    public UsersController(ProblemDetailsFactory problemFactory, ILogger<UsersController> logger,
        IUserService userService) : base(problemFactory, logger)
    {
        _userService = userService;
    }

    /// <summary>Get the full details of a user by id.</summary>
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

    /// <summary>Page through user summaries.</summary>
    /// <remarks>
    /// Supports server-side filtering, sorting, and paging. Page size is clamped
    /// server-side (default 25, max 100).
    /// </remarks>
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
    /// <remarks>
    /// Validation errors (duplicate username/email, password policy violations) are
    /// returned as <c>400 Bad Request</c> with field-keyed details. Conflicts on
    /// unique fields surface as <c>409</c>.
    /// </remarks>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.EditUsers)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> CreateUserAsync([FromBody] UserUpsertRequest model)
    {
        var result = await _userService.CreateAsync(model, CancellationToken);

        return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
    }

    /// <summary>Update a user account's metadata (name, email, enabled flag, etc.).</summary>
    /// <remarks>Does not change the password — use the password endpoint for that.</remarks>
    /// <param name="userId">The id of the user to update.</param>
    /// <param name="model">The updated metadata.</param>
    [HttpPut("{userId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.SystemAdmin.EditUsers)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> UpdateUserAsync([FromRoute] Guid userId, [FromBody] UserUpsertRequest model)
    {
        var result = await _userService.UpdateAsync(userId, model, CancellationToken);

        return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
    }

    /// <summary>Force-set a user's password (admin reset path).</summary>
    /// <remarks>
    /// Bypasses the user's existing password — used when an admin is resetting on
    /// behalf of someone who's locked out. Updates the security stamp, kicking
    /// any active sessions for that user within the validation interval (5 min).
    /// </remarks>
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
    /// <remarks>
    /// System users (the seeded <c>admin</c>) cannot be deleted. Audit data
    /// referencing the user is preserved by retaining the user id.
    /// </remarks>
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