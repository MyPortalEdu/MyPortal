namespace MyPortal.Contracts.Models.People.Contacts;

/// <summary>
/// One of a contact's associated students, shown on the contact profile's Associated Students panel:
/// the student's identity plus the relationship carried on the join. Read-only from the contact side —
/// the relationship is edited from the student's Family tab.
/// </summary>
public class ContactStudentResponse
{
    /// <summary>The StudentContactRelationship (join) id.</summary>
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    /// <summary>"Title First [Middle] Last" of the student, from the legal name.</summary>
    public string StudentName { get; set; } = null!;

    public int AdmissionNumber { get; set; }

    public Guid RelationshipTypeId { get; set; }
    public string? RelationshipTypeName { get; set; }

    public bool HasCorrespondence { get; set; }
    public bool HasParentalResponsibility { get; set; }
    public bool HasPupilReport { get; set; }
    public bool HasCourtOrder { get; set; }

    /// <summary>Emergency-contact priority (1 = first to call). 0 = unranked.</summary>
    public int ContactOrder { get; set; }
}
