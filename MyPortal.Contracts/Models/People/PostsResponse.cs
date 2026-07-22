namespace MyPortal.Contracts.Models.People;

public class PostsResponse
{
    public List<PostResponse> Posts { get; set; } = [];

    public List<LookupResponse> PostCategories { get; set; } = [];
    public List<LookupResponse> ServiceTerms { get; set; } = [];
    public List<LookupResponse> Subjects { get; set; } = [];

    public bool CanEdit { get; set; }
}
