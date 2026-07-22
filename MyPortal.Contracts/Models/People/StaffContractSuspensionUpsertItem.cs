namespace MyPortal.Contracts.Models.People;

/// <summary>
/// One suspension in a contract's replace payload. Null id inserts; populated id updates;
/// omitted rows are soft-deleted.
/// </summary>
public class StaffContractSuspensionUpsertItem
{
    public Guid? Id { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Reason { get; set; }
}
