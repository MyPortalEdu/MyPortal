namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>A DBS disclosure held for a staff member.</summary>
public class DbsCheckResponse
{
    public Guid Id { get; set; }
    public Guid DbsCheckTypeId { get; set; }
    public string CertificateNumber { get; set; } = null!;
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool UpdateServiceEnrolled { get; set; }
    public DateTime? LastUpdateServiceCheck { get; set; }
    public string? Notes { get; set; }
}
