namespace MyPortal.Contracts.Models.People;

/// <summary>
/// An established post in the school's staffing structure, with its vacancy history and how many
/// contracts currently sit against it.
/// </summary>
public class PostResponse
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid? PostCategoryId { get; set; }
    public Guid? ServiceTermId { get; set; }
    public string? SwrPostCode { get; set; }
    public decimal? EstablishedFte { get; set; }

    /// <summary>Contracts currently held against this post — a post in use can't be deleted.</summary>
    public int ContractCount { get; set; }

    /// <summary>True when the post has a vacancy with no end date.</summary>
    public bool IsVacant { get; set; }

    public List<VacancyResponse> Vacancies { get; set; } = [];
}
