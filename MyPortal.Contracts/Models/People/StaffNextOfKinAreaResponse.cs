namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Read payload for the Emergency Contacts area: the staff member's next-of-kin contacts (ordered
/// by call priority) plus the option lists every picker needs, so the editor is self-contained in
/// one fetch. HR-maintained — All-scope view and edit only.
/// </summary>
public class StaffNextOfKinAreaResponse
{
    public List<StaffNextOfKinResponse> Contacts { get; set; } = [];

    public List<LookupResponse> RelationshipTypes { get; set; } = [];
    public List<LookupResponse> PhoneTypes { get; set; } = [];
    public List<LookupResponse> EmailTypes { get; set; } = [];
}
