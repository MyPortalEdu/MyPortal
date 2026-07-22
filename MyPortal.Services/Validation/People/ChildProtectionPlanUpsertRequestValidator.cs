using FluentValidation;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Students;

namespace MyPortal.Services.Validation.People;

public class ChildProtectionPlanUpsertRequestValidator : AbstractValidator<ChildProtectionPlanUpsertRequest>
{
    public ChildProtectionPlanUpsertRequestValidator()
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
