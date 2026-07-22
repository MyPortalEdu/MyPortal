namespace MyPortal.Contracts.Models.Attendance;

public class BulkAttendanceStudentResponse
{
    public Guid StudentId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public int? AdmissionNumber { get; set; }
    
    public bool IsExtra { get; set; }
}
