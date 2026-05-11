using FluentValidation;
using MyPortal.Contracts.Models.Timetabler;

namespace MyPortal.Services.Validation.Timetabler;

public class TimetablePinUpsertRequestValidator : AbstractValidator<TimetablePinUpsertRequest>
{
    public TimetablePinUpsertRequestValidator()
    {
        RuleFor(x => x.CurriculumBlockId).NotEqual(Guid.Empty)
            .WithMessage("CurriculumBlockId is required.");

        // A pin must fix at least one decision variable, otherwise it's a no-op record taking
        // up space and confusing reviewers.
        RuleFor(x => x)
            .Must(p => p.TeacherId.HasValue || p.RoomId.HasValue || p.StartAttendancePeriodId.HasValue)
            .WithMessage("At least one of TeacherId, RoomId, or StartAttendancePeriodId must be specified.");

        // TeacherId and RoomId are per-class assignments — without ClassId there's nothing to
        // attach them to.
        RuleFor(x => x)
            .Must(p => !p.TeacherId.HasValue || p.ClassId.HasValue)
            .WithMessage("TeacherId can only be pinned when ClassId is also specified.");

        RuleFor(x => x)
            .Must(p => !p.RoomId.HasValue || p.ClassId.HasValue)
            .WithMessage("RoomId can only be pinned when ClassId is also specified.");
    }
}
