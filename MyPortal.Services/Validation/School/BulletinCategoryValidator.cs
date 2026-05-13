using System.Text.RegularExpressions;
using FluentValidation;
using MyPortal.Contracts.Models.Bulletins;

namespace MyPortal.Services.Validation.School;

public class BulletinCategoryValidator : AbstractValidator<BulletinCategoryUpsertRequest>
{
    // Accepts 6-digit hex (#RRGGBB) or 8-digit (#RRGGBBAA). Case-insensitive.
    // Matches the DB column shape (NVARCHAR(9)) and what the frontend produces.
    private static readonly Regex HexColour =
        new("^#(?:[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.Compiled);

    public BulletinCategoryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50).WithMessage("Name must not exceed 50 characters.");

        RuleFor(x => x.Icon)
            .NotEmpty().WithMessage("Icon is required.")
            .MaximumLength(50).WithMessage("Icon must not exceed 50 characters.");

        RuleFor(x => x.ColourCode)
            .NotEmpty().WithMessage("Colour is required.")
            .MaximumLength(9).WithMessage("Colour code must not exceed 9 characters.")
            .Must(c => c is null || HexColour.IsMatch(c))
            .WithMessage("Colour must be a hex code like '#6366F1' or '#6366F1FF'.");
    }
}
