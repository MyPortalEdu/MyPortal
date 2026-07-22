namespace MyPortal.Contracts.Models.People.Students;

/// <summary>
/// A derived sibling / family link — another on-roll student who shares at least one contact with
/// this student. Read-only (SIMS "Family Links"); the key is the sibling's Student id for deep-linking.
/// </summary>
public class SiblingResponse
{
    public Guid Id { get; set; }
    public int AdmissionNumber { get; set; }
    public string DisplayName { get; set; } = null!;
}
