using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Staff;
using MyPortal.Contracts.Models.System.Users;
using MyPortal.Services.Interfaces.People;
using MyPortal.Services.Interfaces.System;
using MyPortal.WebApi.Infrastructure.Attributes;

namespace MyPortal.WebApi.Controllers
{
    /// <summary>Current-user endpoints.</summary>
    // Explicit Route overrides BaseApiController's "api/[controller]" pair.
    // Mirrors the dual-route pattern on the base: versioned canonical, unversioned alias.
    [Route("api/me")]
    [Route("api/v{version:apiVersion}/me")]
    public sealed class MeController(
        ProblemDetailsFactory problemFactory,
        ILogger<MeController> logger,
        IUserService userService,
        IStaffTimetableService timetableService,
        ICurrentUser user)
        : BaseApiController(problemFactory, logger)
    {
        /// <summary>Get the current user's profile, user type, and permission set.</summary>
        /// <remarks>Returns <c>401 Unauthorized</c> if the caller is not authenticated.</remarks>
        [HttpGet]
        [ProducesResponseType(typeof(UserInfoResponse), 200)]
        public async Task<IActionResult> GetCurrentUserInfoAsync()
        {
            if (!user.UserId.HasValue)
            {
                return UnauthorizedProblem("Unauthorized");
            }

            var userInfo = await userService.GetInfoByIdAsync(user.UserId.Value, CancellationToken);

            return Ok(userInfo);
        }

        /// <summary>Get the current user's own calendar for a date window.</summary>
        /// <remarks>
        /// Read-only feed for the home dashboard. A staff member gets their full timetable; a user
        /// with no staff record sees the public school diary (plus any events they attend).
        /// Returns <c>401 Unauthorized</c> if the caller is not authenticated.
        /// </remarks>
        /// <param name="from">Inclusive window start.</param>
        /// <param name="to">Exclusive window end.</param>
        [HttpGet("timetable")]
        [ProducesResponseType(typeof(StaffCalendarResponse), 200)]
        public async Task<IActionResult> GetMyTimetableAsync([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (!user.UserId.HasValue)
            {
                return UnauthorizedProblem("Unauthorized");
            }

            var result = await timetableService.GetMyCalendarAsync(from, to, CancellationToken);

            return Ok(result);
        }

        /// <summary>Change the current user's password.</summary>
        /// <remarks>Requires the existing password. For admin resets, use <c>PUT /api/users/{id}/password</c>.</remarks>
        /// <param name="model">Old and new password.</param>
        [HttpPut("password")]
        [ValidateModel]
        [ProducesResponseType(204)]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] UserChangePasswordRequest model)
        {
            if (!user.UserId.HasValue)
            {
                return UnauthorizedProblem("Unauthorized");
            }

            var result = await userService.ChangePasswordAsync(user.UserId.Value, model, CancellationToken);

            return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
        }
    }
}
