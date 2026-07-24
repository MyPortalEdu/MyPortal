using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MyPortal.Auth.Attributes;
using MyPortal.Common.Enums;
using MyPortal.Contracts.Models;
using MyPortal.Contracts.Models.People;
using MyPortal.Services.Interfaces.People;
using MyPortal.WebApi.Infrastructure.Attributes;

namespace MyPortal.WebApi.Controllers;

/// <summary>
/// The training course catalogue, maintained in Staff Setup. Courses are delivered via training
/// events and recorded on certificates.
/// </summary>
public sealed class TrainingCoursesController(
    ProblemDetailsFactory problemFactory,
    ILogger<TrainingCoursesController> logger,
    ITrainingCourseService trainingCourseService)
    : BaseApiController(problemFactory, logger)
{
    /// <summary>List all training courses (active and inactive).</summary>
    [HttpGet]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IReadOnlyList<TrainingCourseResponse>), 200)]
    public async Task<IActionResult> ListAsync()
    {
        var result = await trainingCourseService.ListAsync(CancellationToken);
        return Ok(result);
    }

    /// <summary>Create a training course.</summary>
    [HttpPost]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> CreateAsync([FromBody] TrainingCourseUpsertRequest model)
    {
        var id = await trainingCourseService.CreateAsync(model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Update a training course.</summary>
    [HttpPut("{id:guid}")]
    [ValidateModel]
    [UserType(UserType.Staff)]
    [ProducesResponseType(typeof(IdResponse), 200)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id,
        [FromBody] TrainingCourseUpsertRequest model)
    {
        await trainingCourseService.UpdateAsync(id, model, CancellationToken);
        return Ok(new IdResponse { Id = id });
    }

    /// <summary>Delete a training course (rejected when it is in use).</summary>
    [HttpDelete("{id:guid}")]
    [UserType(UserType.Staff)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id)
    {
        await trainingCourseService.DeleteAsync(id, CancellationToken);
        return NoContent();
    }
}
