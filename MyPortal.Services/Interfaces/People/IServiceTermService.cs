using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

public interface IServiceTermService
{
    Task<ServiceTermsResponse> GetServiceTermsAsync(CancellationToken cancellationToken);

    Task<Guid> CreateServiceTermAsync(ServiceTermUpsertRequest model, CancellationToken cancellationToken);

    Task UpdateServiceTermAsync(Guid id, ServiceTermUpsertRequest model, CancellationToken cancellationToken);

    Task DeleteServiceTermAsync(Guid id, CancellationToken cancellationToken);
}
