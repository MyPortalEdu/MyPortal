namespace MyPortal.Contracts.Models.Attendance;

public class AttendanceCodeResponse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid AttendanceCodeTypeId { get; set; }

    // Restricted codes are returned to every caller so the register UI can display
    // them; only users with the UseRestrictedCodes permission may actually submit
    // marks against them. The UI is expected to grey these out when missing.
    public bool IsRestricted { get; set; }
}
