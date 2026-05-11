using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("TimetableAssignments")]
public class TimetableAssignment : Entity
{
    public Guid TimetableId { get; set; }

    public Guid CurriculumBlockId { get; set; }

    public int SlotIndex { get; set; }

    public Guid ClassId { get; set; }

    public Guid TeacherId { get; set; }

    public Guid? RoomId { get; set; }

    public Guid StartAttendancePeriodId { get; set; }

    [Range(1, int.MaxValue)]
    public int Size { get; set; }

    public Timetable? Timetable { get; set; }
    public CurriculumBlock? CurriculumBlock { get; set; }
    public Class? Class { get; set; }
    public StaffMember? Teacher { get; set; }
    public Room? Room { get; set; }
    public AttendancePeriod? StartAttendancePeriod { get; set; }
}
