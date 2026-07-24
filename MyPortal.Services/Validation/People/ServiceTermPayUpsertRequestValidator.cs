using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class ServiceTermPayUpsertRequestValidator : AbstractValidator<ServiceTermPayUpsertRequest>
{
    public ServiceTermPayUpsertRequestValidator()
    {
        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("An effective date is required.");

        RuleFor(x => x.PointInterval)
            .GreaterThan(0m).When(x => x.PointInterval.HasValue)
            .WithMessage("The point interval must be greater than zero.");

        RuleFor(x => x.MaximumPoint)
            .GreaterThanOrEqualTo(x => x.MinimumPoint!.Value)
            .When(x => x.MaximumPoint.HasValue && x.MinimumPoint.HasValue)
            .WithMessage("The maximum point cannot be below the minimum point.");

        RuleForEach(x => x.Scales).SetValidator(new PayScaleUpsertItemValidator());
    }
}

public class PayScaleUpsertItemValidator : AbstractValidator<PayScaleUpsertItem>
{
    public PayScaleUpsertItemValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("A code is required.")
            .MaximumLength(10).WithMessage("The code must not exceed 10 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A description is required.")
            .MaximumLength(256).WithMessage("The description must not exceed 256 characters.");

        RuleFor(x => x.MinimumPoint)
            .GreaterThan(0m).When(x => x.MinimumPoint.HasValue)
            .WithMessage("The minimum point must be greater than zero.");

        RuleFor(x => x.MaximumPoint)
            .GreaterThanOrEqualTo(x => x.MinimumPoint!.Value)
            .When(x => x.MaximumPoint.HasValue && x.MinimumPoint.HasValue)
            .WithMessage("The maximum point cannot be below the minimum point.");

        // A range is all-or-nothing: half of one would generate an open-ended spine.
        RuleFor(x => x.MaximumPoint)
            .NotNull().When(x => x.MinimumPoint.HasValue)
            .WithMessage("A maximum point is required when a minimum point is set.");

        RuleFor(x => x.MinimumPoint)
            .NotNull().When(x => x.MaximumPoint.HasValue)
            .WithMessage("A minimum point is required when a maximum point is set.");

        RuleFor(x => x.PointInterval)
            .GreaterThan(0m).When(x => x.PointInterval.HasValue)
            .WithMessage("The point interval must be greater than zero.");

        RuleForEach(x => x.Salaries).ChildRules(s =>
        {
            s.RuleFor(x => x.PayZoneId).NotEmpty().WithMessage("A pay zone is required.");
            s.RuleFor(x => x.AnnualSalary)
                .GreaterThanOrEqualTo(0m).WithMessage("A salary cannot be negative.");
        });

        RuleFor(x => x.Salaries)
            .Must(s => s.Select(x => (x.PointValue, x.PayZoneId)).Distinct().Count() == s.Count)
            .WithMessage("Each point and zone can only appear once.");
    }
}
