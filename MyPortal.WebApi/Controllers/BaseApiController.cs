using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyPortal.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected CancellationToken CancellationToken => HttpContext.RequestAborted;
    
    protected IActionResult Error(int statusCode, string message)
    {
        return StatusCode(statusCode, message);
    }

    protected IActionResult Error(HttpStatusCode statusCode, string message)
    {
        return Error((int) statusCode, message);
    }
}