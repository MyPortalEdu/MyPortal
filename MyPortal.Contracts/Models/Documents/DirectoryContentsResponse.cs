namespace MyPortal.Contracts.Models.Documents
{
    public class DirectoryContentsResponse
    {
        public DirectoryDetailsResponse Directory { get; set; }
        public IReadOnlyList<DirectoryDetailsResponse> Directories { get; set; }
        public IReadOnlyList<DocumentDetailsResponse> Documents { get; set; }
    }
}
