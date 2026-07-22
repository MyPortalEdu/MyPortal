namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Whole-area replace for the Emergency Contacts area: the next-of-kin links reconciled by id
/// (null id inserts, populated id updates, omitted rows soft-delete the link). HR-edit only.
/// </summary>
public class StaffNextOfKinAreaUpsertRequest
{
    public List<StaffNextOfKinUpsertItem> Contacts { get; set; } = [];
}
