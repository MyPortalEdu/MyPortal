namespace MyPortal.Contracts.Models.Documents
{
    public class DocumentContentResponse
    {
        public required DocumentDetailsResponse Details { get; set; }
        public required Stream Content { get; set; }
    }
}
