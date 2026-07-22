namespace MyPortal.Contracts.Models.People;

/// <summary>
/// The Staff Setup posts register: every established post plus the option lists the editor needs.
/// </summary>
public class PostsResponse
{
    public List<PostResponse> Posts { get; set; } = [];

    public List<LookupResponse> PostCategories { get; set; } = [];
    public List<LookupResponse> ServiceTerms { get; set; } = [];
    public List<LookupResponse> Subjects { get; set; } = [];

    /// <summary>True when the caller may maintain the register.</summary>
    public bool CanEdit { get; set; }
}
