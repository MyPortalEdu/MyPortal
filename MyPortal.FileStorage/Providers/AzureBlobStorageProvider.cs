using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using MyPortal.Common.Options;
using MyPortal.FileStorage.Interfaces;

namespace MyPortal.FileStorage.Providers
{
    public sealed class AzureBlobStorageProvider : IFileStorageProvider
    {
        private readonly BlobContainerClient _containerClient;

        public AzureBlobStorageProvider(IOptions<FileStorageOptions> options)
        {
            var azureOptions = options.Value?.AzureBlob
                               ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(azureOptions.ConnectionString))
                throw new InvalidOperationException("FileStorage:Azure:ConnectionString is not configured.");
            if (string.IsNullOrWhiteSpace(azureOptions.Container))
                throw new InvalidOperationException("FileStorage:Azure:Container is not configured.");

            var serviceClient = new BlobServiceClient(azureOptions.ConnectionString);
            _containerClient = serviceClient.GetBlobContainerClient(azureOptions.Container);
        }

        public async Task SaveFileAsync(string storageKey, Stream content, string contentType, CancellationToken cancellationToken)
        {
            await EnsureContainerExistsAsync(cancellationToken);

            var blobClient = _containerClient.GetBlobClient(storageKey);

            var headers = new BlobHttpHeaders
            {
                ContentType = contentType
            };

            await blobClient.UploadAsync(content, headers, cancellationToken: cancellationToken);
        }

        public async Task<Stream> OpenReadFileAsync(string storageKey, CancellationToken cancellationToken)
        {
            await EnsureContainerExistsAsync(cancellationToken);

            var blobClient = _containerClient.GetBlobClient(storageKey);
            if (!await blobClient.ExistsAsync(cancellationToken))
                throw new FileNotFoundException("Blob not found for storage key.", storageKey);

            var response = await blobClient.DownloadAsync(cancellationToken);
            return response.Value.Content;
        }

        public async Task DeleteFileAsync(string storageKey, CancellationToken cancellationToken)
        {
            await EnsureContainerExistsAsync(cancellationToken);

            var blobClient = _containerClient.GetBlobClient(storageKey);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        }

        public async Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken)
        {
            await EnsureContainerExistsAsync(cancellationToken);

            var blobClient = _containerClient.GetBlobClient(storageKey);
            var exists = await blobClient.ExistsAsync(cancellationToken);
            return exists.Value;
        }
        
        private async Task EnsureContainerExistsAsync(CancellationToken ct)
        {
            await _containerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: ct);
        }
    }
}
