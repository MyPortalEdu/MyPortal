namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A certificate covering an absence — a self-certification, a doctor's fit note, or a
/// return-to-work record.
/// </summary>
public class StaffAbsenceCertificateResponse
{
    public Guid Id { get; set; }
    public DateTime DateReceived { get; set; }
    public DateTime? DateSigned { get; set; }
    public bool IsSelfCertified { get; set; }
    public bool IsReturnToWork { get; set; }
    public string? SignedBy { get; set; }
    public string? Notes { get; set; }
}
