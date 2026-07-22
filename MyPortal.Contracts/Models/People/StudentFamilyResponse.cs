namespace MyPortal.Contracts.Models.People;

/// <summary>
/// The Family area of the student profile: the student's contacts (priority-ordered) with their
/// relationship details, the derived sibling links, and the relationship-type option list so the
/// editor is self-contained in one fetch.
/// </summary>
public class StudentFamilyResponse
{
    public List<StudentContactRelationshipResponse> Contacts { get; set; } = [];
    public List<SiblingResponse> Siblings { get; set; } = [];
    public List<LookupResponse> RelationshipTypes { get; set; } = [];
}
