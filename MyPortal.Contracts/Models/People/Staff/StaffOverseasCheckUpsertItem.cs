namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>An overseas-check row in the upsert. Null id is a new row; populated
/// id updates; omitted rows are soft-deleted server-side.</summary>
public class StaffOverseasCheckUpsertItem
{
    public Guid? Id { get; set; }
    public Guid NationalityId { get; set; }
    public DateTime? CheckedDate { get; set; }
    public bool IsClear { get; set; }
    public string? Notes { get; set; }
}
