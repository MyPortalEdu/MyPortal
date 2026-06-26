namespace MyPortal.Contracts.Models.People;

/// <summary>
/// One qualification in a professional-details replace payload. A null <see cref="Id"/> is a new
/// row; a populated <see cref="Id"/> updates the existing one. Rows present in the database but
/// absent from the payload are soft-deleted.
/// </summary>
public class StaffQualificationUpsertItem
{
    public Guid? Id { get; set; }
    public Guid? QualificationLevelId { get; set; }
    public string Title { get; set; } = null!;
    public string? Subject { get; set; }
    public string? AwardingBody { get; set; }
    public string? Grade { get; set; }
    public Guid? ClassOfDegreeId { get; set; }
    public int? YearAwarded { get; set; }
}
