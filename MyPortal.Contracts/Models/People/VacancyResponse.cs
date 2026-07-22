namespace MyPortal.Contracts.Models.People;

/// <summary>A period an established post was unfilled. A null <see cref="EndDate"/> is still open.</summary>
public class VacancyResponse
{
    public Guid Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsAdvertised { get; set; }
    public bool IsTemporarilyFilled { get; set; }
    public Guid? SubjectId { get; set; }
    public string? Notes { get; set; }
}
