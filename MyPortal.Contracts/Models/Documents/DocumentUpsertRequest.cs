namespace MyPortal.Contracts.Models.Documents
{
    public class DocumentUpsertRequest
    {
        public Guid TypeId { get; set; }
        public Guid DirectoryId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public bool IsPrivate { get; set; }

        // File storage params - not required from UI, populated by controller from IFormFile
        public string? FileName { get; set; }
        public Stream? Content { get; set; }
        public string? ContentType { get; set; }
        public long? SizeBytes { get; set; }
    }
}
