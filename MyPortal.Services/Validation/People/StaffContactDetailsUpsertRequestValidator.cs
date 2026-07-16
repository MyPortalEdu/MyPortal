using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class StaffContactDetailsUpsertRequestValidator : AbstractValidator<StaffContactDetailsUpsertRequest>
{
    public StaffContactDetailsUpsertRequestValidator()
    {
        RuleForEach(x => x.Emails).ChildRules(email =>
        {
            email.RuleFor(e => e.TypeId)
                .NotEmpty().WithMessage("Email type is required.");

            email.RuleFor(e => e.Address)
                .NotEmpty().WithMessage("Email address is required.")
                .EmailAddress().WithMessage("Enter a valid email address.")
                .MaximumLength(128).WithMessage("Email address must not exceed 128 characters.");

            email.RuleFor(e => e.Notes)
                .MaximumLength(256).WithMessage("Notes must not exceed 256 characters.");
        });

        RuleForEach(x => x.Phones).ChildRules(phone =>
        {
            phone.RuleFor(p => p.TypeId)
                .NotEmpty().WithMessage("Phone type is required.");

            phone.RuleFor(p => p.Number)
                .NotEmpty().WithMessage("Phone number is required.")
                .MaximumLength(128).WithMessage("Phone number must not exceed 128 characters.");
        });

        // At most one row per list may be the main — the UI enforces this too, this is the guard.
        RuleFor(x => x.Emails)
            .Must(emails => emails.Count(e => e.IsMain) <= 1)
            .WithMessage("Only one email can be marked as the main one.");

        RuleFor(x => x.Phones)
            .Must(phones => phones.Count(p => p.IsMain) <= 1)
            .WithMessage("Only one phone number can be marked as the main one.");
    }
}
