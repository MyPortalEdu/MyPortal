namespace MyPortal.Contracts.Models.Attendance;

public class AttendanceCodeResponse
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid AttendanceCodeTypeId { get; set; }
    
    public bool IsRestricted { get; set; }
}
