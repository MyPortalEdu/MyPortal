using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models.Curriculum;
using MyPortal.Services.Interfaces.Curriculum;
using MyPortal.WebApi.Infrastructure.Attributes;

namespace MyPortal.WebApi.Controllers;

public sealed class AcademicYearsController : BaseApiController<AcademicYearsController>
{
    private readonly IAcademicYearService _academicYearService;

    public AcademicYearsController(ProblemDetailsFactory problemFactory,
        ILogger<AcademicYearsController> logger, IAcademicYearService academicYearService)
        : base(problemFactory, logger)
    {
        _academicYearService = academicYearService;
    }

    [HttpGet]
    [Permission(PermissionMode.RequireAny, Permissions.Curriculum.ViewAcademicYears)]
    public async Task<IActionResult> ListAsync()
    {
        var result = await _academicYearService.ListAsync(CancellationToken);
        return Ok(result);
    }

    [HttpGet("current")]
    [Permission(PermissionMode.RequireAny, Permissions.Curriculum.ViewAcademicYears)]
    public async Task<IActionResult> GetCurrentAsync()
    {
        var result = await _academicYearService.GetCurrentAsync(CancellationToken);
        // Null = no AY has started yet. 204 keeps the contract simple — clients
        // either get an AY summary body or know there isn't one.
        return result is null ? NoContent() : Ok(result);
    }

    [HttpGet("{academicYearId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Curriculum.ViewAcademicYears)]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid academicYearId)
    {
        var result = await _academicYearService.GetByIdAsync(academicYearId, CancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Curriculum.EditAcademicYears)]
    public async Task<IActionResult> CreateAsync([FromBody] AcademicYearUpsertRequest model)
    {
        var id = await _academicYearService.CreateAcademicYear(model, CancellationToken);
        return Ok(new { id });
    }

    [HttpPut("{academicYearId:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Curriculum.EditAcademicYears)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid academicYearId,
        [FromBody] AcademicYearUpsertRequest model)
    {
        var id = await _academicYearService.UpdateAcademicYear(academicYearId, model, CancellationToken);
        return Ok(new { id });
    }

    [HttpDelete("{academicYearId:guid}")]
    [UserType(UserType.Staff)]
    [Permission(PermissionMode.RequireAny, Permissions.Curriculum.EditAcademicYears)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid academicYearId)
    {
        await _academicYearService.DeleteAcademicYear(academicYearId, CancellationToken);
        return NoContent();
    }
}
