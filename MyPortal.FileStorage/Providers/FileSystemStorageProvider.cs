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
                throw new FileNotFoundException("File not found for storage key.", storageKey);

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
            if (string.IsNullOrWhiteSpace(storageKey))
                throw new UnauthorizedAccessException("Storage key must not be empty.");

            // Reject keys that look like absolute paths, drive letters, or UNC paths before any join.
            if (Path.IsPathRooted(storageKey)
                || storageKey.StartsWith('/')
                || storageKey.StartsWith('\\')
                || storageKey.Contains(':'))
            {
                throw new UnauthorizedAccessException($"Access to path '{storageKey}' is denied.");
            }

            var safeKey = storageKey.Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_rootPath, safeKey);

            // Normalize paths to resolve any relative path components
            var normalizedFullPath = Path.GetFullPath(fullPath);
            var normalizedRootPath = Path.GetFullPath(_rootPath);

            if (!normalizedRootPath.EndsWith(Path.DirectorySeparatorChar))
            {
                normalizedRootPath += Path.DirectorySeparatorChar;
            }

            // Filesystem case-sensitivity differs by OS — match the platform default rather than
            // forcing Ordinal (which falsely rejects on case-insensitive volumes).
            var comparison = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            if (!normalizedFullPath.StartsWith(normalizedRootPath, comparison))
            {
                throw new UnauthorizedAccessException($"Access to path '{storageKey}' is denied. Path traversal detected.");
            }

            return normalizedFullPath;
        }
    }
}
