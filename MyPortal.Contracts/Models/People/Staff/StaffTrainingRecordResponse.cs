namespace MyPortal.Contracts.Models.People.Staff;

/// <summary>A CPD / training certificate held for a staff member.</summary>
public class StaffTrainingRecordResponse
{
    public Guid Id { get; set; }
    public Guid TrainingCourseId { get; set; }
    public Guid StatusId { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Provider { get; set; }
    public decimal? Hours { get; set; }
    public string? CertificateReference { get; set; }
}
