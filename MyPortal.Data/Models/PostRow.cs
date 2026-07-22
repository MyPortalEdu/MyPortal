namespace MyPortal.Data.Models;

/// <summary>A post plus the number of contracts currently held against it.</summary>
public class PostRow
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid? PostCategoryId { get; set; }
    public Guid? ServiceTermId { get; set; }
    public string? SwrPostCode { get; set; }
    public decimal? EstablishedFte { get; set; }
    public int ContractCount { get; set; }
}
