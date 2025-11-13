namespace MyPortal.Contracts.Models.Documents
{
    public class DirectoryDetailsResponse
    {
        public Guid Id { get; set; }
        public Guid? ParentId { get; set; }
        public string Name { get; set; } = null!;
        public bool IsPrivate { get; set; }
    }
}
