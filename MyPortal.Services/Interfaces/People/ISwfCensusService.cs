using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

public interface ISwfCensusService
{
    Task<SwfCensusPreviewResponse> GetPreviewAsync(DateTime? referenceDate, CancellationToken cancellationToken);

    Task<string> GenerateXmlAsync(DateTime? referenceDate, CancellationToken cancellationToken);
}
