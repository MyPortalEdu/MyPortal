using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Auth.Interfaces;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.Services;

namespace MyPortal.WebApi.Controllers
{
    [Route("api/me")]
    public class MeController : BaseApiController
    {
        private readonly IUserService _userService;
        private readonly ICurrentUser _user;

        public MeController(IValidationService validationService, IUserService userService, ICurrentUser user) : base(validationService)
        {
            _userService = userService;
            _user = user;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            if (!_user.UserId.HasValue)
            {
                return Unauthorized();
            }

            var userInfo = await _userService.GetInfoByIdAsync(_user.UserId.Value, CancellationToken);

            return Ok(userInfo);
        }
    }
}
