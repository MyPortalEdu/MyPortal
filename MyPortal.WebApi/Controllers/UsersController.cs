using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.System.Users;
using MyPortal.Services.Interfaces.Services;
using MyPortal.WebApi.Infrastructure.Attributes;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

public class UsersController : BaseApiController<UsersController>
{
    private readonly IUserService _userService;

    public UsersController(ProblemDetailsFactory problemFactory, ILogger<UsersController> logger,
        IUserService userService) : base(problemFactory, logger)
    {
        _userService = userService;
    }

    [HttpGet("{userId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.System.ViewUsers)]
    [ProducesResponseType(typeof(UserDetailsDto), 200)]
    public async Task<IActionResult> GetUserDetailsByIdAsync([FromRoute] Guid userId)
    {
        var result = await _userService.GetDetailsByIdAsync(userId, CancellationToken);

        if (result == null)
        {
            return NotFoundProblem("User not found.");
        }
        
        return Ok(result);
    }

    [HttpGet]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.System.ViewUsers)]
    [ProducesResponseType(typeof(PageResult<UserSummaryDto>), 200)]
    public async Task<IActionResult> GetUsersAsync([FromQuery] FilterOptions filter, [FromQuery] SortOptions sort, [FromQuery] PageOptions paging)
    {
        var result = await _userService.GetUsersAsync(filter, sort, paging, CancellationToken);
        
        return Ok(result);
    }
    
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.System.EditUsers)]
    public async Task<IActionResult> CreateUserAsync([FromBody] UserUpsertDto model)
    {
        var result = await _userService.CreateUserAsync(model, CancellationToken);
        
        return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
    }

    [HttpPut("{userId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.System.EditUsers)]
    public async Task<IActionResult> UpdateUserAsync([FromRoute] Guid userId, [FromBody] UserUpsertDto model)
    {
        var result = await _userService.UpdateUserAsync(userId, model, CancellationToken);
        
        return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
    }

    [HttpPut("{userId:guid}/password")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.System.EditUsers)]
    public async Task<IActionResult> SetPasswordAsync([FromRoute] Guid userId, [FromBody] UserSetPasswordDto model)
    {
        var result = await _userService.SetPasswordAsync(userId, model, CancellationToken);
        
        return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
    }

    [HttpDelete("{userId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.System.EditUsers)]
    public async Task<IActionResult> DeleteUserAsync([FromRoute] Guid userId)
    {
        var result = await _userService.DeleteUserAsync(userId, CancellationToken);
        
        return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
    }
}