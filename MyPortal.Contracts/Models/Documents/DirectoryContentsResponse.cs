namespace MyPortal.Contracts.Models.Documents
{
    public class DirectoryContentsResponse
    {
        public DirectoryDetailsResponse Directory { get; set; }

        public IReadOnlyList<DirectoryDetailsResponse> Directories { get; set; } =
            Array.Empty<DirectoryDetailsResponse>();

        public IReadOnlyList<DocumentDetailsResponse> Documents { get; set; } = Array.Empty<DocumentDetailsResponse>();
    }
}
