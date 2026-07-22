using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class PepUpsertRequestValidator : AbstractValidator<PepUpsertRequest>
{
    public PepUpsertRequestValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("A start date is required.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("The end date must be on or after the start date.");

        RuleFor(x => x.Comment)
            .MaximumLength(1024);
    }
}
