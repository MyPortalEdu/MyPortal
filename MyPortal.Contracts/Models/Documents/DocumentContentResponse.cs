namespace MyPortal.Contracts.Models.Documents
{
    public class DocumentContentResponse
    {
        public DocumentDetailsResponse Details { get; set; }
        public Stream Content { get; set; }
    }
}
