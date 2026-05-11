using System.ComponentModel.DataAnnotations;

namespace MyPortal.Contracts.Models.Attendance;

public class BulkAttendanceMarkUpsert
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid AttendanceWeekId { get; set; }

    [Required]
    public Guid AttendancePeriodId { get; set; }

    // Null signals "delete the existing mark for this (Student, Week, Period) cell".
    // Used by reception staff correcting a mark by clearing it rather than overwriting.
    public Guid? AttendanceCodeId { get; set; }

    [StringLength(256)]
    public string? Comments { get; set; }

    [Range(1, int.MaxValue)]
    public int? MinutesLate { get; set; }
}
