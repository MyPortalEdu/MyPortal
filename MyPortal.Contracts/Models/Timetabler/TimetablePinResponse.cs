namespace MyPortal.Contracts.Models.Timetabler;

public class TimetablePinResponse
{
    public Guid Id { get; set; }
    public Guid TimetableId { get; set; }
    public Guid CurriculumBlockId { get; set; }
    public int SlotIndex { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? TeacherId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? StartAttendancePeriodId { get; set; }
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
}
