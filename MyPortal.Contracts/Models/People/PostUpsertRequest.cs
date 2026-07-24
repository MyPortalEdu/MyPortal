namespace MyPortal.Contracts.Models.People;

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
