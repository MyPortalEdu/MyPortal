using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface ITrainingCourseRepository : IEntityRepository<TrainingCourse>
{
    Task<IReadOnlyList<TrainingCourseResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<bool> IsReferencedAsync(Guid id, CancellationToken cancellationToken);
}
