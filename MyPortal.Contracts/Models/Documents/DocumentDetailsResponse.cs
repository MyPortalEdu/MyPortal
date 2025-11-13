using System.ComponentModel.DataAnnotations;

namespace MyPortal.Contracts.Models.Documents
{
    public class DocumentDetailsResponse
    {
        public Guid Id { get; set; }

        public Guid TypeId { get; set; }

        public string TypeDescription { get; set; }

        public Guid DirectoryId { get; set; }

        public string CreatedByName { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public string LastModifiedByName { get; set; } = null!;

        public DateTime LastModifiedAt { get; set; }

        public string StorageKey { get; set; } = null!;

        public string FileName { get; set; } = null!;

        public string ContentType { get; set; } = null!;

        public long? SizeBytes { get; set; }

        public string? Hash { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public bool IsPrivate { get; set; }

        public bool IsDeleted { get; set; }
    }
}
