namespace MyPortal.Contracts.Models.People;

public class StaffNextOfKinAreaResponse
{
    public List<StaffNextOfKinResponse> Contacts { get; set; } = [];

    public List<LookupResponse> RelationshipTypes { get; set; } = [];
    public List<LookupResponse> PhoneTypes { get; set; } = [];
    public List<LookupResponse> EmailTypes { get; set; } = [];
}
