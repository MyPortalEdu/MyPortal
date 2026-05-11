namespace MyPortal.Contracts.Models.Attendance;

public class BulkAttendanceStudentResponse
{
    public Guid StudentId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public int? AdmissionNumber { get; set; }

    // True when this student appears solely via SessionExtraNames (no regular
    // StudentGroupMembership in the requested range). Useful for the UI to mark
    // them visually distinct from the regular roster.
    public bool IsExtra { get; set; }
}
