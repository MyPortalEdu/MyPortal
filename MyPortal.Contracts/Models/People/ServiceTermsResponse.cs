namespace MyPortal.Contracts.Models.People;

public class ServiceTermsResponse
{
    public List<ServiceTermResponse> ServiceTerms { get; set; } = [];

    public List<LookupResponse> SuperannuationSchemes { get; set; } = [];

    public bool CanEdit { get; set; }
}
