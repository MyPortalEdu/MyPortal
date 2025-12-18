namespace MyPortal.WebApi.Models.Documents
{
    public class DocumentUpsertForm
    {
        public Guid TypeId { get; set; }
        public Guid DirectoryId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsPrivate { get; set; }
        public IFormFile? File { get; set; }
    }
}
