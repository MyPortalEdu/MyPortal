using MyPortal.Common.Enums;

namespace MyPortal.Common.Options
{
    public sealed class FileStorageOptions
    {
        public FileStorageProvider Provider { get; init; } = FileStorageProvider.FileSystem;
        
        /// <summary>
        /// Maximum file size for document uploads in bytes. Defaults to 50 MB.
        /// </summary>
        public long MaxFileSizeBytes { get; set; } = Constants.DocumentLimits.MaxFileSizeBytes;
        
        public FileSystemOptions? FileSystem { get; set; }
        public AzureBlobOptions? AzureBlob { get; set; }
    }
}
