using MyPortal.Contracts.Models.Pastoral;

namespace MyPortal.Services.Interfaces.Pastoral;

public interface IHouseService
{
    Task<Guid> CreateHouseAsync(HouseUpsertRequest model, CancellationToken cancellationToken);
    Task UpdateHouseAsync(Guid id, HouseUpsertRequest model, CancellationToken cancellationToken);
    Task DeleteHouseAsync(Guid id, CancellationToken cancellationToken);
}