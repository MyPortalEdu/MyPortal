using FluentValidation;
using MyPortal.Contracts.Models.Bulletins;
using MyPortal.Services.Interfaces.Providers;

namespace MyPortal.Services.Validation.School;

public class BulletinValidator : AbstractValidator<BulletinUpsertRequest>
{
    public BulletinValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(50).WithMessage("Title must not exceed 50 characters.");

        RuleFor(x => x.Detail)
            .NotEmpty().WithMessage("Detail is required.")
            .MaximumLength(2000).WithMessage("Detail must not exceed 2000 characters.");

        // Use the injected provider so validation and BulletinAccessPolicy share one clock —
        // and so tests can pin time without flake.
        RuleFor(y => y.ExpiresAt)
            .Must(exp => exp == null || exp > dateTimeProvider.UtcNow)
            .WithMessage("Expiry date must be in the future.");
    }
}