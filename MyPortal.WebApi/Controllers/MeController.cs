using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Auth.Models;
using MyPortal.Services.Interfaces;

namespace MyPortal.WebApi.Controllers
{
    [Route("api/me")]
    public class MeController : BaseApiController
    {
        private readonly UserManager<ApplicationUser> _users;

        public MeController(IValidationService validationService, UserManager<ApplicationUser> users) : base(validationService)
        {
            _users = users;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            var user = await _users.GetUserAsync(User);

            if (user is null)
            {
                return Unauthorized();
            }

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.UserType,
                user.IsEnabled
            });
        }
    }
}
