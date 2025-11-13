using Microsoft.Extensions.Options;
using MyPortal.Common.Options;
using MyPortal.FileStorage.Interfaces;

namespace MyPortal.FileStorage.Providers
{
    public class FileSystemStorageProvider : IFileStorageProvider
    {
        private readonly string _rootPath;

        public FileSystemStorageProvider(IOptions<FileStorageOptions> options)
        {
            var op = options.Value?.FileSystem ?? throw new ArgumentNullException(nameof(options));
            _rootPath = op.RootPath ??
                        throw new InvalidOperationException("FileStorage:FileSystem:RootPath is not configured.");

            if (string.IsNullOrWhiteSpace(_rootPath))
            {
                throw new InvalidOperationException("RootPath must not be empty.");
            }
        }

        public async Task SaveFileAsync(string storageKey, Stream content, string contentType, CancellationToken cancellationToken)
        {
            var fullPath = GetFullPath(storageKey);
            var directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory!);
            }

            await using var file = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await content.CopyToAsync(file, cancellationToken);
        }

        public Task<Stream> OpenReadFileAsync(string storageKey, CancellationToken cancellationToken)
        {
            var fullPath = GetFullPath(storageKey);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("File not found for storage key.", fullPath);

            Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult(stream);
        }

        public Task DeleteFileAsync(string storageKey, CancellationToken cancellationToken)
        {
            var fullPath = GetFullPath(storageKey);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default)
        {
            var fullPath = GetFullPath(storageKey);
            var exists = File.Exists(fullPath);
            return Task.FromResult(exists);
        }

        private string GetFullPath(string storageKey)
        {
            var safeKey = storageKey.Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(_rootPath, safeKey);
        }
    }
}
