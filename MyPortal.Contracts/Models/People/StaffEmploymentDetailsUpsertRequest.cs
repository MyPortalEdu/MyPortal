namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Write payload for the Employment Details area. The employments list (and each spell's
/// contracts) is a whole-collection replace: the server diffs against what's stored — inserting
/// new rows, updating matched ids, and soft-deleting anything no longer present.
/// </summary>
public class StaffEmploymentDetailsUpsertRequest
{
    public string? BankName { get; set; }
    public string? BankAccount { get; set; }
    public string? BankSortCode { get; set; }
    public string? NiNumber { get; set; }

    public List<StaffEmploymentUpsertItem> Employments { get; set; } = [];
}
