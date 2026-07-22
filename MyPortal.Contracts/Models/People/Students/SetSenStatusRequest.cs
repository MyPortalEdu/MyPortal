namespace MyPortal.Contracts.Models.People.Students;

/// <summary>Write payload to set the student's current SEN status. Closes the open status-history row
/// and opens a new one from StartDate, refreshing the cached status on the Student.</summary>
public class SetSenStatusRequest
{
    public Guid SenStatusId { get; set; }
    public DateTime StartDate { get; set; }
}
