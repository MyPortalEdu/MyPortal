namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinCategoryUpsertRequest
{
    public required string Name { get; set; }
    public required string Icon { get; set; }
    public required string ColourCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool Active { get; set; } = true;
    public long ExpectedVersion { get; set; }
}
