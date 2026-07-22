using FluentValidation;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Students;

namespace MyPortal.Services.Validation.People;

public class SenNeedUpsertRequestValidator : AbstractValidator<SenNeedUpsertRequest>
{
    public SenNeedUpsertRequestValidator()
    {
        RuleFor(x => x.SenTypeId)
            .NotEmpty()
            .WithMessage("A SEN type must be selected.");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("A start date is required.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("The resolved date must be on or after the start date.");

        RuleFor(x => x.Rank)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Rank must be 1 or greater.");

        RuleFor(x => x.Description)
            .MaximumLength(1024);
    }
}
