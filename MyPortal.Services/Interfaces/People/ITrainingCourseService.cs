using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

public interface ITrainingCourseService
{
    Task<IReadOnlyList<TrainingCourseResponse>> ListAsync(CancellationToken cancellationToken);

    Task<Guid> CreateAsync(TrainingCourseUpsertRequest model, CancellationToken cancellationToken);

    Task UpdateAsync(Guid id, TrainingCourseUpsertRequest model, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
