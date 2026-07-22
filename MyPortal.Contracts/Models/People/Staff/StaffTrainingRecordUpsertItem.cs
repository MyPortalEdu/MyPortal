namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>A training record row in the upsert. Null id is a new row; populated id updates;
/// omitted rows are removed server-side.</summary>
public class StaffTrainingRecordUpsertItem
{
    public Guid? Id { get; set; }
    public Guid TrainingCourseId { get; set; }
    public Guid StatusId { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Provider { get; set; }
    public decimal? Hours { get; set; }
    public string? CertificateReference { get; set; }
}
