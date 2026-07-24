using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

public interface IPayScaleService
{
    Task<ServiceTermPayResponse> GetServiceTermPayAsync(Guid serviceTermId, DateTime? effectiveFrom,
        CancellationToken cancellationToken);

    Task UpdateServiceTermPayAsync(Guid serviceTermId, ServiceTermPayUpsertRequest model,
        CancellationToken cancellationToken);

    Task DeletePayScaleAsync(Guid payScaleId, CancellationToken cancellationToken);

    Task<PayAwardPreviewResponse> PreviewPayAwardAsync(Guid serviceTermId, PayAwardRequest model,
        CancellationToken cancellationToken);

    Task ApplyPayAwardAsync(Guid serviceTermId, PayAwardRequest model, CancellationToken cancellationToken);
}
