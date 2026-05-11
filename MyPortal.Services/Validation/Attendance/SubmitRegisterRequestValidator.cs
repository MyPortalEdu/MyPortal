using FluentValidation;
using MyPortal.Contracts.Models.Attendance;

namespace MyPortal.Services.Validation.Attendance;

public class SubmitRegisterRequestValidator : AbstractValidator<SubmitRegisterRequest>
{
    public SubmitRegisterRequestValidator()
    {
        RuleFor(x => x.Marks)
            .Cascade(CascadeMode.Stop)
            .NotNull().WithMessage("Marks are required.")
            .Must(marks => marks.Select(m => m.StudentId).Distinct().Count() == marks.Count)
                .WithMessage("Each student may appear only once in a register submission.");

        RuleForEach(x => x.Marks).SetValidator(new SubmitMarkRequestValidator());
    }
}

public class SubmitMarkRequestValidator : AbstractValidator<SubmitMarkRequest>
{
    public SubmitMarkRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEqual(Guid.Empty).WithMessage("Student is required.");

        RuleFor(x => x.AttendanceCodeId)
            .NotEqual(Guid.Empty).WithMessage("Attendance code is required for every mark.");

        RuleFor(x => x.Comments)
            .MaximumLength(256).WithMessage("Comments must not exceed 256 characters.");

        RuleFor(x => x.MinutesLate)
            .GreaterThan(0).When(x => x.MinutesLate.HasValue)
            .WithMessage("Minutes late must be a positive number.");
    }
}
