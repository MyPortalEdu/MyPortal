using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("TimetablePins")]
public class TimetablePin : Entity
{
    public Guid TimetableId { get; set; }

    public Guid CurriculumBlockId { get; set; }

    public int SlotIndex { get; set; }

    public Guid? ClassId { get; set; }

    public Guid? TeacherId { get; set; }

    public Guid? RoomId { get; set; }

    public Guid? StartAttendancePeriodId { get; set; }

    public Guid CreatedById { get; set; }
    public string CreatedByIpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Timetable? Timetable { get; set; }
    public CurriculumBlock? CurriculumBlock { get; set; }
    public Class? Class { get; set; }
    public StaffMember? Teacher { get; set; }
    public Room? Room { get; set; }
    public AttendancePeriod? StartAttendancePeriod { get; set; }
    public User? CreatedBy { get; set; }
}
