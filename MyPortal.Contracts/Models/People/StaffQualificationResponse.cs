namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A single academic / professional qualification held by a staff member.
/// <see cref="QualificationLevelId"/> and <see cref="ClassOfDegreeId"/> resolve against the
/// option lists on <see cref="StaffProfessionalDetailsResponse"/>.
/// </summary>
public class StaffQualificationResponse
{
    public Guid Id { get; set; }
    public Guid? QualificationLevelId { get; set; }
    public string Title { get; set; } = null!;
    public string? Subject { get; set; }
    public string? AwardingBody { get; set; }
    public string? Grade { get; set; }
    public Guid? ClassOfDegreeId { get; set; }
    public int? YearAwarded { get; set; }
}
