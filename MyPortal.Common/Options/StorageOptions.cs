namespace MyPortal.Common.Options
{
    public sealed class StorageOptions
    {
        public string Provider { get; init; } = "Local";
        public string? InstallDirectory { get; init; }
        public string? FileEncryptionKey { get; init; }
    }
}
