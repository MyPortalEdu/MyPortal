using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Enums;
using MyPortal.Common.Constants;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.People;
using MyPortal.Services.Interfaces.People;
using MyPortal.WebApi.Infrastructure.Attributes;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// Student-record detail endpoints (the staff-facing student profile). Access is flat / role-based
/// via the <c>[Permission]</c> attribute — a granted staff viewer sees any student. See
/// docs/student-profile-access.md.
/// </summary>
[UserType(UserType.Staff)]
public sealed class StudentsController(
    ProblemDetailsFactory problemFactory,
    ILogger<StudentsController> logger,
    IStudentService studentService)
    : BaseApiController(problemFactory, logger)
{
    /// <summary>Page through student summaries for the student list / picker.</summary>
    [HttpGet]
    [Permission(PermissionMode.RequireAny, Permissions.Student.ViewStudentBasicDetails)]
    [ProducesResponseType(typeof(PageResult<StudentSummaryResponse>), 200)]
    public async Task<IActionResult> GetStudentsAsync([FromQuery] int page, [FromQuery] int pageSize,
        [FromQuery] FilterOptions? filter, [FromQuery] SortOptions? sort)
    {
        var options = GetListingOptions(page, pageSize, filter, sort);

        var result = await studentService.GetStudentsAsync(options.FilterOptions, options.SortOptions,
            options.PageOptions, CancellationToken);

        return Ok(result);
    }

    /// <summary>Get the student profile header (identity, admission number, status). 404 if not found.</summary>
    /// <param name="studentId">The Student id.</param>
    [HttpGet("{studentId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Student.ViewStudentBasicDetails)]
    [ProducesResponseType(typeof(StudentHeaderResponse), 200)]
    public async Task<IActionResult> GetHeaderAsync([FromRoute] Guid studentId)
    {
        var result = await studentService.GetHeaderAsync(studentId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Get the basic details area.</summary>
    /// <param name="studentId">The Student id.</param>
    [HttpGet("{studentId:guid}/basic-details")]
    [Permission(PermissionMode.RequireAny, Permissions.Student.ViewStudentBasicDetails)]
    [ProducesResponseType(typeof(StudentBasicDetailsResponse), 200)]
    public async Task<IActionResult> GetBasicDetailsAsync([FromRoute] Guid studentId)
    {
        var result = await studentService.GetBasicDetailsAsync(studentId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Update the basic details area.</summary>
    /// <param name="studentId">The Student id.</param>
    /// <param name="model">The new basic-details payload.</param>
    [HttpPut("{studentId:guid}/basic-details")]
    [ValidateModel]
    [Permission(PermissionMode.RequireAny, Permissions.Student.EditStudentBasicDetails)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateBasicDetailsAsync([FromRoute] Guid studentId,
        [FromBody] StudentBasicDetailsUpsertRequest model)
    {
        await studentService.UpdateBasicDetailsAsync(studentId, model, CancellationToken);
        return Ok(new IdResponse { Id = studentId });
    }

    /// <summary>Get the registration area.</summary>
    /// <param name="studentId">The Student id.</param>
    [HttpGet("{studentId:guid}/registration")]
    [Permission(PermissionMode.RequireAny, Permissions.Student.ViewStudentRegistration)]
    [ProducesResponseType(typeof(StudentRegistrationDetailsResponse), 200)]
    public async Task<IActionResult> GetRegistrationDetailsAsync([FromRoute] Guid studentId)
    {
        var result = await studentService.GetRegistrationDetailsAsync(studentId, CancellationToken);
        return Ok(result);
    }

    /// <summary>Update the registration area.</summary>
    /// <param name="studentId">The Student id.</param>
    /// <param name="model">The new registration payload.</param>
    [HttpPut("{studentId:guid}/registration")]
    [ValidateModel]
    [Permission(PermissionMode.RequireAny, Permissions.Student.EditStudentRegistration)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateRegistrationDetailsAsync([FromRoute] Guid studentId,
        [FromBody] StudentRegistrationDetailsUpsertRequest model)
    {
        await studentService.UpdateRegistrationDetailsAsync(studentId, model, CancellationToken);
        return Ok(new IdResponse { Id = studentId });
    }

    /// <summary>Generate a suggested permanent UPN for the registration editor. Returns 400 if the
    /// school's LA / establishment number aren't configured.</summary>
    [HttpPost("generate-upn")]
    [Permission(PermissionMode.RequireAny, Permissions.Student.EditStudentRegistration)]
    [ProducesResponseType(typeof(GeneratedUpnResponse), 200)]
    public async Task<IActionResult> GenerateUpnAsync()
    {
        var result = await studentService.GenerateUpnAsync(CancellationToken);
        return Ok(result);
    }

    /// <summary>Add or replace the student's photo. The image is resized server-side.</summary>
    /// <param name="studentId">The Student id.</param>
    /// <param name="file">The uploaded image (multipart form field <c>file</c>).</param>
    [HttpPut("{studentId:guid}/photo")]
    [Permission(PermissionMode.RequireAny, Permissions.Student.EditStudentBasicDetails)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> SetPhotoAsync([FromRoute] Guid studentId, IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequestProblem("No file was uploaded.");
        }

        await using var stream = file.OpenReadStream();
        await studentService.SetPhotoAsync(studentId, stream, file.ContentType, file.FileName, CancellationToken);
        return Ok(new IdResponse { Id = studentId });
    }

    /// <summary>Stream the student's photo. 404 if none.</summary>
    /// <param name="studentId">The Student id.</param>
    [HttpGet("{studentId:guid}/photo")]
    [Permission(PermissionMode.RequireAny, Permissions.Student.ViewStudentBasicDetails)]
    public async Task<IActionResult> GetPhotoAsync([FromRoute] Guid studentId)
    {
        var content = await studentService.GetPhotoAsync(studentId, CancellationToken);

        var typedHeaders = Response.GetTypedHeaders();
        if (!string.IsNullOrWhiteSpace(content.Details.Hash))
        {
            typedHeaders.ETag = new Microsoft.Net.Http.Headers.EntityTagHeaderValue($"\"{content.Details.Hash}\"");
        }
        typedHeaders.LastModified = content.Details.LastModifiedAt;
        Response.Headers["X-Content-Type-Options"] = "nosniff";

        var safeContentType = SafeContentTypes.Sanitize(content.Details.ContentType);
        return File(content.Content, safeContentType, content.Details.FileName,
            enableRangeProcessing: content.Content.CanSeek);
    }

    /// <summary>Remove the student's photo.</summary>
    /// <param name="studentId">The Student id.</param>
    [HttpDelete("{studentId:guid}/photo")]
    [Permission(PermissionMode.RequireAny, Permissions.Student.EditStudentBasicDetails)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> DeletePhotoAsync([FromRoute] Guid studentId)
    {
        await studentService.DeletePhotoAsync(studentId, CancellationToken);
        return Ok(new IdResponse { Id = studentId });
    }

    /// <summary>Create a new student (Person + Student) from basic details. Admission number is
    /// auto-assigned. Returns the new Student id.</summary>
    /// <param name="model">The basic-details payload.</param>
    [HttpPost]
    [ValidateModel]
    [Permission(PermissionMode.RequireAny, Permissions.Student.EditStudentBasicDetails)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateAsync([FromBody] StudentBasicDetailsUpsertRequest model)
    {
        var id = await studentService.CreateAsync(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Search existing People for the "new student" flow, so someone already on file gets a
    /// student role rather than a duplicate Person. Empty for blank/too-short queries.</summary>
    /// <param name="query">The search term.</param>
    [HttpGet("person-matches")]
    [Permission(PermissionMode.RequireAny, Permissions.Student.EditStudentBasicDetails)]
    [ProducesResponseType(typeof(IReadOnlyList<StudentMatchResponse>), 200)]
    public async Task<IActionResult> SearchPeopleAsync([FromQuery] string? query)
    {
        var result = await studentService.SearchPeopleAsync(query, CancellationToken);
        return Ok(result);
    }

    /// <summary>Attach a student role to an existing Person. Returns the new Student id.</summary>
    /// <param name="model">The person to attach a student role to.</param>
    [HttpPost("for-person")]
    [ValidateModel]
    [Permission(PermissionMode.RequireAny, Permissions.Student.EditStudentBasicDetails)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateForPersonAsync([FromBody] StudentCreateForPersonRequest model)
    {
        var id = await studentService.CreateForPersonAsync(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Soft-delete a student. The underlying Person is left intact.</summary>
    /// <param name="studentId">The Student id.</param>
    [HttpDelete("{studentId:guid}")]
    [Permission(PermissionMode.RequireAny, Permissions.Student.EditStudentBasicDetails)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid studentId)
    {
        await studentService.DeleteAsync(studentId, CancellationToken);
        return NoContent();
    }
}
