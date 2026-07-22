namespace MyPortal.Contracts.Models.People;

/// <summary>
/// Write payload for assigning (or clearing, when null) a staff member's line manager. HR-owned:
/// the server rejects self-assignment and any target that would create a cycle in the chain.
/// </summary>
public class SetStaffLineManagerRequest
{
    public Guid? LineManagerId { get; set; }
}
