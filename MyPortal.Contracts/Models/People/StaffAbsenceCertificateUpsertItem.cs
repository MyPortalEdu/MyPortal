namespace MyPortal.Contracts.Models.People;

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
