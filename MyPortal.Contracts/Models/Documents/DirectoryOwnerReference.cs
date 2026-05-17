namespace MyPortal.Contracts.Models.Documents;

/// Identifies the entity that holds an FK reference to a directory.
/// Returned by usp_directory_get_referencing_owner when the directory
/// is the root of an IDirectoryEntity.
public class DirectoryOwnerReference
{
    public string OwnerType { get; set; } = null!;
    public Guid OwnerId { get; set; }
}
