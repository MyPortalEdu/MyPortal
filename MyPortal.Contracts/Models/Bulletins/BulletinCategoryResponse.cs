namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinCategoryResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Icon { get; set; }
    public required string ColourCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool Active { get; set; }
    public bool IsSystem { get; set; }
    public long Version { get; set; }
}
