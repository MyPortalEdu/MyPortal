namespace MyPortal.Data.Models;

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
