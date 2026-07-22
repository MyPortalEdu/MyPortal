namespace MyPortal.Contracts.Models.People;

/// <summary>
/// A single medical condition on the student Medical area — the condition (FK), whether it requires
/// medication and, if so, what; plus onset / resolved / info-received dates and free-text notes.
/// A resolved condition (non-null <see cref="EndDate"/>) is kept on record as history. Used in both
/// the read and write payloads.
/// </summary>
public class PersonConditionItem
{
    public Guid MedicalConditionId { get; set; }

    public bool RequiresMedication { get; set; }
    public string? Medication { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? InfoReceivedDate { get; set; }
    public string? Notes { get; set; }
}
