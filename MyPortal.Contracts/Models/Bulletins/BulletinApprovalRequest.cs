namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinApprovalRequest
{
    public bool IsApproved { get; set; }

    /// <summary>
    /// Optimistic-concurrency token: the version the approver believes it is approving.
    /// Prevents an approval silently overwriting a concurrent edit.
    /// </summary>
    public long ExpectedVersion { get; set; }
}