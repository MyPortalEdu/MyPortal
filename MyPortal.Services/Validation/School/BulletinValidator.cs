using FluentValidation;
using MyPortal.Contracts.Models.Bulletins;

namespace MyPortal.Services.Validation.School;

public class BulletinValidator : AbstractValidator<BulletinUpsertRequest>
{
    public BulletinValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(50).WithMessage("Title must not exceed 50 characters.");
        
        RuleFor(x => x.Detail)
            .NotEmpty().WithMessage("Detail is required");
        
        RuleFor(y => y.ExpiresAt)
            .Must(exp => exp == null || exp > DateTime.UtcNow)
            .WithMessage("Expiry date must be in the future.");
    }
}