using MyPortal.Contracts.Models.Bulletins;

namespace MyPortal.Services.Interfaces.School;

public interface IBulletinCategoryService
{
    Task<IList<BulletinCategoryResponse>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<BulletinCategoryResponse> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<Guid> CreateAsync(BulletinCategoryUpsertRequest model, CancellationToken cancellationToken);
    Task UpdateAsync(Guid categoryId, BulletinCategoryUpsertRequest model, CancellationToken cancellationToken);
    Task DeleteAsync(Guid categoryId, CancellationToken cancellationToken);
}
