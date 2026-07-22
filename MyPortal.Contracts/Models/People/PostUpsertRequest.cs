namespace MyPortal.Contracts.Models.People;

/// <summary>Create / update payload for an established post, including its vacancies.</summary>
public class PostUpsertRequest
{
    public string Reference { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid? PostCategoryId { get; set; }
    public Guid? ServiceTermId { get; set; }
    public string? SwrPostCode { get; set; }
    public decimal? EstablishedFte { get; set; }

    public List<VacancyUpsertItem> Vacancies { get; set; } = [];
}
