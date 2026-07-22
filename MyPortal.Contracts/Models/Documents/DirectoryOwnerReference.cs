namespace MyPortal.Contracts.Models.Documents;

public class DirectoryOwnerReference
{
    public string OwnerType { get; set; } = null!;
    public Guid OwnerId { get; set; }
}
