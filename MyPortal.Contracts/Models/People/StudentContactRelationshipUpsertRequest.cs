namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Write payload to link a contact to a student (or update an existing link). The contact must
/// already exist (created/searched via the Contacts area); this only manages the relationship join.
/// </summary>
public class StudentContactRelationshipUpsertRequest
{
    public Guid ContactId { get; set; }
    public Guid RelationshipTypeId { get; set; }

    public bool HasCorrespondence { get; set; }
    public bool HasParentalResponsibility { get; set; }
    public bool HasPupilReport { get; set; }
    public bool HasCourtOrder { get; set; }

    /// <summary>Emergency-contact priority (1 = first to call). 0 = unranked.</summary>
    public int ContactOrder { get; set; }
}
