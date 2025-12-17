using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Interfaces;
using MyPortal.Contracts.Models.System.Users;
using MyPortal.Services.Interfaces.Services;
using MyPortal.WebApi.Infrastructure.Attributes;

namespace MyPortal.WebApi.Controllers
{
    [Route("api/me")]
    public class MeController : BaseApiController<MeController>
    {
        private readonly IUserService _userService;
        private readonly ICurrentUser _user;

        public MeController(ProblemDetailsFactory problemFactory, ILogger<MeController> logger,
            IUserService userService, ICurrentUser user) : base(problemFactory, logger)
        {
            _userService = userService;
            _user = user;
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrentUserInfoAsync()
        {
            if (!_user.UserId.HasValue)
            {
                return UnauthorizedProblem("Unauthorized");
            }

            var userInfo = await _userService.GetInfoByIdAsync(_user.UserId.Value, CancellationToken);

            return Ok(userInfo);
        }

        [HttpPut("password")]
        [ValidateModel]
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
