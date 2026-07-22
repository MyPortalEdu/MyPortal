namespace MyPortal.Contracts.Models.People.Students;

/// <summary>
/// One of a student's family/contact links, as shown on the Family tab: the contact's identity plus
/// the relationship flags carried on the join (correspondence, parental responsibility, pupil report,
/// court order) and the emergency-contact priority order.
/// </summary>
public class StudentContactRelationshipResponse
{
    /// <summary>The StudentContactRelationship (join) id — the key for update/remove.</summary>
    public Guid Id { get; set; }

    public Guid ContactId { get; set; }

    /// <summary>"Title First [Middle] Last" of the contact, from the legal name.</summary>
    public string ContactName { get; set; } = null!;

    public string? JobTitle { get; set; }

    public Guid RelationshipTypeId { get; set; }
    public string? RelationshipTypeName { get; set; }

    public bool HasCorrespondence { get; set; }
    public bool HasParentalResponsibility { get; set; }
    public bool HasPupilReport { get; set; }
    public bool HasCourtOrder { get; set; }

    /// <summary>Emergency-contact priority (1 = first to call). 0 = unranked.</summary>
    public int ContactOrder { get; set; }
}
