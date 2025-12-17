namespace MyPortal.Contracts.Models.Documents
{
    public class DocumentUpsertRequest : IDisposable, IAsyncDisposable
    {
        public Guid TypeId { get; set; }
        public Guid DirectoryId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsPrivate { get; set; }

        // File storage params - not required from UI, populated by controller from IFormFile
        public string? FileName { get; set; }
        public Stream? Content { get; set; }
        public string? ContentType { get; set; }
        public long? SizeBytes { get; set; }

        public void Dispose()
        {
            Content?.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (Content != null) await Content.DisposeAsync();
        }
    }
}
