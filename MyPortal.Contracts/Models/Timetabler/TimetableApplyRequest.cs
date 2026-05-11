using System.ComponentModel.DataAnnotations;

namespace MyPortal.Contracts.Models.Timetabler;

public class TimetableApplyRequest
{
    /// First date the new timetable becomes effective. The previous Active timetable
    /// (if any) has its EffectiveTo set to (this date - 1 day).
    [Required]
    public DateTime EffectiveFrom { get; set; }

    /// Optional end date — leave null to run until superseded.
    public DateTime? EffectiveTo { get; set; }
}
