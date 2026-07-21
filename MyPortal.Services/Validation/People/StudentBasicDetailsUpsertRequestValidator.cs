using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class StudentBasicDetailsUpsertRequestValidator : AbstractValidator<StudentBasicDetailsUpsertRequest>
{
    public StudentBasicDetailsUpsertRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(256).WithMessage("First name must not exceed 256 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(256).WithMessage("Last name must not exceed 256 characters.");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Gender is required.")
            .MaximumLength(1).WithMessage("Gender must be a single-character code.");

        RuleFor(x => x.Title)
            .MaximumLength(128).WithMessage("Title must not exceed 128 characters.");

        RuleFor(x => x.MiddleName)
            .MaximumLength(256).WithMessage("Middle name must not exceed 256 characters.");

        RuleFor(x => x.PreferredFirstName)
            .MaximumLength(256).WithMessage("Preferred first name must not exceed 256 characters.");

        RuleFor(x => x.PreferredLastName)
            .MaximumLength(256).WithMessage("Preferred last name must not exceed 256 characters.");
    }
}
