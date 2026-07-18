namespace MyPortal.Contracts.Models.Documents
{
    public class DirectoryContentsResponse(
        DirectoryDetailsResponse directory,
        IReadOnlyList<DirectoryDetailsResponse> directories,
        IReadOnlyList<DocumentDetailsResponse> documents)
    {
        public DirectoryDetailsResponse Directory { get; set; } = directory;

        public IReadOnlyList<DirectoryDetailsResponse> Directories { get; set; } = directories;

        public IReadOnlyList<DocumentDetailsResponse> Documents { get; set; } = documents;
    }
}
