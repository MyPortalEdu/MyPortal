using MyPortal.Common.Enums;

namespace MyPortal.Common.Options
{
    public sealed class FileStorageOptions
    {
        public FileStorageProvider Provider { get; init; } = FileStorageProvider.FileSystem;
        public FileSystemOptions? FileSystem { get; set; }
        public AzureBlobOptions? AzureBlob { get; set; }
    }
}
