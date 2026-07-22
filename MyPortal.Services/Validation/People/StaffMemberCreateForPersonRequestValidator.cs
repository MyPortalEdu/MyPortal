using FluentValidation;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Staff;

namespace MyPortal.Services.Validation.People;

public class StaffMemberCreateForPersonRequestValidator : AbstractValidator<StaffMemberCreateForPersonRequest>
{
    public StaffMemberCreateForPersonRequestValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("A person must be selected.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Staff code is required.")
            .MaximumLength(128).WithMessage("Staff code must not exceed 128 characters.");
    }
}
