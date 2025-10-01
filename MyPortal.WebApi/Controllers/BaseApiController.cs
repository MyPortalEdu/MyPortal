using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Services.Interfaces;

namespace MyPortal.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private readonly IValidationService _validationService;

    public BaseApiController(IValidationService validationService)
    {
        _validationService = validationService;
    }
    
    protected CancellationToken CancellationToken => HttpContext.RequestAborted;

    protected async Task ValidateAsync<T>(T model)
    {
        await _validationService.ValidateAsync(model);
    }
    
    protected IActionResult Error(int statusCode, string message)
    {
        return StatusCode(statusCode, message);
    }

    protected IActionResult Error(HttpStatusCode statusCode, string message)
    {
        return Error((int) statusCode, message);
    }
}