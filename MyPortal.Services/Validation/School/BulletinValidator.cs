using FluentValidation;
using MyPortal.Common.Enums;
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

        RuleFor(x => x.CategoryId)
            .NotEqual(Guid.Empty).WithMessage("Category is required.");

        // Use the injected provider so validation and downstream code share one clock —
        // and so tests can pin time without flake.
        RuleFor(x => x.ExpiresAt)
            .Must(exp => exp == null || exp > dateTimeProvider.UtcNow)
            .WithMessage("Expiry date must be in the future.");

        RuleFor(x => x.Audiences)
            .NotEmpty().WithMessage("At least one audience is required.");

        RuleForEach(x => x.Audiences).ChildRules(child =>
        {
            child.RuleFor(a => a)
                .Must(a => a.AudienceKind != BulletinAudienceKind.StudentGroup || a.StudentGroupId.HasValue)
                .WithMessage("StudentGroup audience entries require a StudentGroupId.");

            child.RuleFor(a => a)
                .Must(a => a.AudienceKind == BulletinAudienceKind.StudentGroup || !a.StudentGroupId.HasValue)
                .WithMessage("StudentGroupId must only be set when AudienceKind is StudentGroup.");
        });
    }
}
