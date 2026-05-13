namespace MyPortal.Contracts.Models.Bulletins;

public class BulletinPinRequest
{
    public bool IsPinned { get; set; }

    /// <summary>
    /// Optimistic-concurrency token: the version the caller believes it is pinning.
    /// Prevents a pin silently overwriting a concurrent edit.
    /// </summary>
    public long ExpectedVersion { get; set; }
}
