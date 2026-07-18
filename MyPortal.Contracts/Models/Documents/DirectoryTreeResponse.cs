namespace MyPortal.Contracts.Models.Documents;

public class DirectoryTreeResponse(
    DirectoryDetailsResponse directory,
    IReadOnlyList<DirectoryTreeResponse> directories,
    IReadOnlyList<DocumentDetailsResponse> documents)
{
    public DirectoryDetailsResponse Directory { get; set; } = directory;

    public IReadOnlyList<DirectoryTreeResponse> Directories { get; set; } = directories;

    public IReadOnlyList<DocumentDetailsResponse> Documents { get; set; } = documents;
}