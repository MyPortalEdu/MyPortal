namespace MyPortal.FileStorage.Interfaces
{
    public interface IFileStorageProvider
    {
        Task SaveFileAsync(string storageKey, Stream content, string contentType, CancellationToken cancellationToken);
        Task<Stream> OpenReadFileAsync(string storageKey, CancellationToken cancellationToken);
        Task DeleteFileAsync(string storageKey, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(string storageKey, CancellationToken cancellationToken);
    }
}
