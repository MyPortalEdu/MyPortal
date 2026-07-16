namespace MyPortal.Contracts.Models.People;

/// <summary>
/// One email in a contact-details replace payload. A null <see cref="Id"/> is a new address;
/// a populated <see cref="Id"/> updates the existing row. Rows present in the database but
/// absent from the payload are soft-deleted.
/// </summary>
public class PersonEmailUpsertItem
{
    public Guid? Id { get; set; }
    public Guid TypeId { get; set; }
    public string Address { get; set; } = null!;
    public bool IsMain { get; set; }
    public string? Notes { get; set; }
}
