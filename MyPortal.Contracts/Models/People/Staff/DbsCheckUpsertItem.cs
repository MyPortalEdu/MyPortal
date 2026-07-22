namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>A DBS row in the upsert. Null id is a new row; populated id updates;
/// omitted rows are soft-deleted server-side.</summary>
public class DbsCheckUpsertItem
{
    public Guid? Id { get; set; }
    public Guid DbsCheckTypeId { get; set; }
    public string CertificateNumber { get; set; } = null!;
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool UpdateServiceEnrolled { get; set; }
    public DateTime? LastUpdateServiceCheck { get; set; }
    public string? Notes { get; set; }
}
