using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Models.Documents
{
    public class DirectoryUpsertRequest
    {
        public Guid? ParentId { get; set; } = null;
        public string Name { get; set; } = null!;
        public bool IsPrivate { get; set; }
        public DirectoryUploadPolicy UploadPolicy { get; set; }
    }
}
