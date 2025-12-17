namespace MyPortal.Contracts.Models.Documents
{
    public class DirectoryContentsResponse
    {
        public DirectoryContentsResponse(DirectoryDetailsResponse directory,
            IReadOnlyList<DirectoryDetailsResponse> directories, IReadOnlyList<DocumentDetailsResponse> documents)
        {
            Directory = directory;
            Directories = directories;
            Documents = documents;
        }

        public DirectoryDetailsResponse Directory { get; set; }

        public IReadOnlyList<DirectoryDetailsResponse> Directories { get; set; }

        public IReadOnlyList<DocumentDetailsResponse> Documents { get; set; }
    }
}
