using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class StaffResponsibilitiesUpsertRequestValidator : AbstractValidator<StaffResponsibilitiesUpsertRequest>
{
    public StaffResponsibilitiesUpsertRequestValidator()
    {
        RuleForEach(x => x.Responsibilities).ChildRules(responsibility =>
        {
            responsibility.RuleFor(r => r.ResponsibilityTypeId)
                .NotEmpty().WithMessage("A responsibility is required.");

            responsibility.RuleFor(r => r.StartDate)
                .NotEmpty().WithMessage("A start date is required.");

            // End date is optional (a current responsibility has none), but if set it can't precede the start.
            responsibility.RuleFor(r => r.EndDate)
                .GreaterThanOrEqualTo(r => r.StartDate)
                .When(r => r.EndDate.HasValue)
                .WithMessage("The end date cannot be before the start date.");

            responsibility.RuleFor(r => r.Notes)
                .MaximumLength(256).WithMessage("Notes must not exceed 256 characters.");
        });
    }
}
