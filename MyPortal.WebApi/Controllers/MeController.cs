using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Interfaces;
using MyPortal.Contracts.Models.System.Users;
using MyPortal.Services.Interfaces.System;
using MyPortal.WebApi.Infrastructure.Attributes;
// (UserInfoResponse / UserChangePasswordRequest live in MyPortal.Contracts.Models.System.Users)

namespace MyPortal.WebApi.Controllers
{
    /// <summary>
    /// Endpoints for the currently signed-in user. The Angular SPA hits
    /// <c>GET /api/me</c> on bootstrap to populate user type, permissions, and
    /// display info; every other call should be on a controller scoped to the
    /// resource being acted on, not here.
    /// </summary>
    [Route("api/me")]
    public sealed class MeController : BaseApiController<MeController>
    {
        private readonly IUserService _userService;
        private readonly ICurrentUser _user;

        public MeController(ProblemDetailsFactory problemFactory, ILogger<MeController> logger,
            IUserService userService, ICurrentUser user) : base(problemFactory, logger)
        {
            _userService = userService;
            _user = user;
        }

        /// <summary>Get the current user's profile, user type, and permission set.</summary>
        /// <remarks>
        /// Returns <c>401 Unauthorized</c> if the caller isn't authenticated. The
        /// SPA caches this for the session via <c>shareReplay(1)</c> and clears the
        /// cache on 401 from any other endpoint.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(UserInfoResponse), 200)]
        public async Task<IActionResult> GetCurrentUserInfoAsync()
        {
            if (!_user.UserId.HasValue)
            {
                return UnauthorizedProblem("Unauthorized");
            }

            var userInfo = await _userService.GetInfoByIdAsync(_user.UserId.Value, CancellationToken);

            return Ok(userInfo);
        }

        /// <summary>Change the current user's password (self-service).</summary>
        /// <remarks>
        /// Requires the user's existing password as proof. For admin-driven resets
        /// see <c>PUT /api/users/{id}/password</c>. Updates the security stamp on
        /// success, which ends every other active session for this user within the
        /// validation interval (5 min).
        /// </remarks>
        /// <param name="model">Old and new password.</param>
        [HttpPut("password")]
        [ValidateModel]
        [ProducesResponseType(204)]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] UserChangePasswordRequest model)
        {
            if (!_user.UserId.HasValue)
            {
                return UnauthorizedProblem("Unauthorized");
            }

            var result = await _userService.ChangePasswordAsync(_user.UserId.Value, model, CancellationToken);

            return !result.Succeeded ? IdentityResultProblem(result) : NoContent();
        }
    }
}
