using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Interfaces.People;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

public class TrainingCourseService(
    IAuthorizationService authorizationService,
    ILogger<TrainingCourseService> logger,
    ITrainingCourseRepository trainingCourseRepository)
    : BaseService(authorizationService, logger), ITrainingCourseService
{
    public async Task<IReadOnlyList<TrainingCourseResponse>> ListAsync(CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewStaffSetup, cancellationToken);
        return await trainingCourseRepository.GetAllAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(TrainingCourseUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditStaffSetup, cancellationToken);

        var course = new TrainingCourse
        {
            Id = SqlConvention.SequentialGuid(),
            Code = model.Code.Trim(),
            Name = model.Name.Trim(),
            Description = Describe(model),
            Active = model.Active
        };
        await trainingCourseRepository.InsertAsync(course, cancellationToken);
        return course.Id;
    }

    public async Task UpdateAsync(Guid id, TrainingCourseUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditStaffSetup, cancellationToken);

        var course = await trainingCourseRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Training course not found.");

        course.Code = model.Code.Trim();
        course.Name = model.Name.Trim();
        course.Description = Describe(model);
        course.Active = model.Active;
        await trainingCourseRepository.UpdateAsync(course, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditStaffSetup, cancellationToken);

        _ = await trainingCourseRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Training course not found.");

        if (await trainingCourseRepository.IsReferencedAsync(id, cancellationToken))
        {
            throw new EntityInUseException(
                "This course is used by a training event or certificate — deactivate it instead of deleting.");
        }

        await trainingCourseRepository.DeleteAsync(id, cancellationToken, softDelete: false);
    }

    // Description is NOT NULL on the lookup; fall back to the name when none is given.
    private static string Describe(TrainingCourseUpsertRequest model) =>
        string.IsNullOrWhiteSpace(model.Description) ? model.Name.Trim() : model.Description.Trim();
}
