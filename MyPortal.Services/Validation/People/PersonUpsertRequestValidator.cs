using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class PersonUpsertRequestValidator : AbstractValidator<PersonUpsertRequest>
{
    public PersonUpsertRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(256).WithMessage("First name must not exceed 256 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(256).WithMessage("Last name must not exceed 256 characters.");

        // Single-character ISO-style gender code, mirroring Person.Gender's [StringLength(1)].
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

        RuleFor(x => x.NhsNumber)
            .MaximumLength(10).WithMessage("NHS number must not exceed 10 characters.");
    }
}
