namespace MyPortal.Contracts.Models.People;

/// <summary>A single staff absence or leave record.</summary>
public class StaffAbsenceResponse
{
    public Guid Id { get; set; }
    public Guid AbsenceTypeId { get; set; }
    public Guid? IllnessTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsConfidential { get; set; }
    public string? Notes { get; set; }
}
