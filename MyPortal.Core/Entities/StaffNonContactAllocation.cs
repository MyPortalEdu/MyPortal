using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("StaffNonContactAllocations")]
public class StaffNonContactAllocation : Entity
{
    public Guid StaffMemberId { get; set; }

    public Guid? TimetableId { get; set; }

    public Guid AttendancePeriodId { get; set; }

    [Required, StringLength(20)]
    public string Code { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public StaffMember? StaffMember { get; set; }
    public Timetable? Timetable { get; set; }
    public AttendancePeriod? AttendancePeriod { get; set; }
}
