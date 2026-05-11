using System.ComponentModel.DataAnnotations;

namespace MyPortal.Contracts.Models.Attendance;

public class SubmitMarkRequest
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid AttendanceCodeId { get; set; }

    [StringLength(256)]
    public string? Comments { get; set; }

    [Range(1, int.MaxValue)]
    public int? MinutesLate { get; set; }
}
