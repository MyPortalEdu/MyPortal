using System.ComponentModel.DataAnnotations;

namespace MyPortal.Contracts.Models.People;

public class TrainingEventUpsertRequest
{
    [Required]
    public Guid TrainingCourseId { get; set; }

    [StringLength(200)]
    public string? Title { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(200)]
    public string? Trainer { get; set; }

    [StringLength(200)]
    public string? Provider { get; set; }

    public decimal? Hours { get; set; }

    public int? Capacity { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

public class BookTrainingAttendeesRequest
{
    [Required]
    public List<Guid> StaffMemberIds { get; set; } = [];
}

public class SetTrainingAttendanceRequest
{
    public bool Attended { get; set; }
}
