using System.ComponentModel.DataAnnotations;

namespace MyPortal.Contracts.Models.Timetabler;

public class TimetableRunRequest
{
    [Required]
    public Guid WeekPatternId { get; set; }
}
