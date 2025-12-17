namespace MyPortal.Contracts.Models.Documents;

public class DirectoryTreeResponse
{
    public DirectoryTreeResponse(DirectoryDetailsResponse directory,
        IReadOnlyList<DirectoryTreeResponse> directories, IReadOnlyList<DocumentDetailsResponse> documents)
    {
        Directory = directory;
        Directories = directories;
        Documents = documents;
    }

    public DirectoryDetailsResponse Directory { get; set; }
    
    public IReadOnlyList<DirectoryTreeResponse> Directories { get; set; }

    public IReadOnlyList<DocumentDetailsResponse> Documents { get; set; }
}