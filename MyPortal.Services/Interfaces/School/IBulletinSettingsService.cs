using MyPortal.Contracts.Models.Bulletins;

namespace MyPortal.Services.Interfaces.School;

public interface IBulletinSettingsService
{
    Task<BulletinSettingsResponse> GetAsync(CancellationToken cancellationToken);
    Task UpdateAsync(BulletinSettingsUpdateRequest model, CancellationToken cancellationToken);
}
