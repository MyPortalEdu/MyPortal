namespace MyPortal.Contracts.Models.Agencies;

public class AgencyUpsertRequest
{
    public Guid AgencyTypeId { get; set; }

    public Guid DirectoryId { get; set; }
    
    public string Name { get; set; } = null!;
    
    public string? Website { get; set; }

    public long ExpectedVersion { get; set; }
}