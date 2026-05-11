using System.ComponentModel.DataAnnotations;

namespace MyPortal.Contracts.Models.Timetabler;

public class TimetablePinUpsertRequest
{
    [Required]
    public Guid CurriculumBlockId { get; set; }

    [Range(0, int.MaxValue)]
    public int SlotIndex { get; set; }

    /// Required when pinning a Teacher or Room (those are per-class). Leave null for a
    /// block-level pin that only fixes the slot's start period.
    public Guid? ClassId { get; set; }

    public Guid? TeacherId { get; set; }

    public Guid? RoomId { get; set; }

    public Guid? StartAttendancePeriodId { get; set; }
}
