namespace MyPortal.Contracts.Models.People;

/// <summary>
/// One certificate in an absence's replace payload. A null <see cref="Id"/> is a new row; a
/// populated one updates. Certificates absent from the payload are deleted with the absence's
/// reconcile (hard delete, matching the lean absence tables).
/// </summary>
public class StaffAbsenceCertificateUpsertItem
{
    public Guid? Id { get; set; }
    public DateTime DateReceived { get; set; }
    public DateTime? DateSigned { get; set; }
    public bool IsSelfCertified { get; set; }
    public bool IsReturnToWork { get; set; }
    public string? SignedBy { get; set; }
    public string? Notes { get; set; }
}
