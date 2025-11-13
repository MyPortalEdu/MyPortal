namespace MyPortal.Common.Options
{
    public sealed class AzureBlobOptions
    {
        public string ConnectionString { get; set; } = default!;
        public string Container { get; set; } = "myportal-documents";
    }
}
