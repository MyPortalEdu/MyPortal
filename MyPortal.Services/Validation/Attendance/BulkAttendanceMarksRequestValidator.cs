using FluentValidation;
using MyPortal.Contracts.Models.Attendance;

namespace MyPortal.Services.Validation.Attendance;

public class BulkAttendanceMarksRequestValidator : AbstractValidator<BulkAttendanceMarksRequest>
{
    public BulkAttendanceMarksRequestValidator()
    {
        RuleFor(x => x.StudentGroupId)
            .NotEqual(Guid.Empty).WithMessage("Student group is required.");

        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .WithMessage("From date must be on or before To date.");

        RuleFor(x => x.Marks)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Marks are required.")
            // The TVP enforces this at the data layer too via PK, but rejecting
            // up-front gives a much clearer error than the SQL TVP violation.
            .Must(marks => marks
                .Select(m => (m.StudentId, m.AttendanceWeekId, m.AttendancePeriodId))
                .Distinct()
                .Count() == marks.Count)
            .WithMessage("Each (student, week, period) cell may only appear once.");

        RuleForEach(x => x.Marks).SetValidator(new BulkAttendanceMarkUpsertValidator());
    }
}

public class BulkAttendanceMarkUpsertValidator : AbstractValidator<BulkAttendanceMarkUpsert>
{
    public BulkAttendanceMarkUpsertValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEqual(Guid.Empty).WithMessage("Student is required.");

        RuleFor(x => x.AttendanceWeekId)
            .NotEqual(Guid.Empty).WithMessage("Attendance week is required.");

        RuleFor(x => x.AttendancePeriodId)
            .NotEqual(Guid.Empty).WithMessage("Attendance period is required.");

        // Empty Guid would round-trip to NULL and look like a delete; force the
        // caller to be explicit about deletes by leaving the property unset.
        RuleFor(x => x.AttendanceCodeId)
            .NotEqual(Guid.Empty)
            .When(x => x.AttendanceCodeId.HasValue)
            .WithMessage("Attendance code, when supplied, must not be empty.");

        RuleFor(x => x.Comments)
            .MaximumLength(256).WithMessage("Comments must not exceed 256 characters.");

        RuleFor(x => x.MinutesLate)
            .GreaterThan(0).When(x => x.MinutesLate.HasValue)
            .WithMessage("Minutes late must be a positive number.");

        // A delete shouldn't carry comments / minutes-late; they have no meaning
        // without a code. Reject so the caller can't misread our DELETE semantics.
        RuleFor(x => x)
            .Must(x => string.IsNullOrEmpty(x.Comments) && !x.MinutesLate.HasValue)
            .When(x => !x.AttendanceCodeId.HasValue)
            .WithMessage("Comments and minutes-late are not allowed when deleting a mark (AttendanceCodeId is null).");
    }
}
