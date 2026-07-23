using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class PayAwardRequestValidator : AbstractValidator<PayAwardRequest>
{
    public PayAwardRequestValidator()
    {
        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("An effective date is required.");

        RuleFor(x => x.SourceEffectiveFrom)
            .NotEmpty().WithMessage("A generation to uplift from is required.");

        RuleFor(x => x.EffectiveFrom)
            .GreaterThan(x => x.SourceEffectiveFrom)
            .WithMessage("The pay award must take effect after the generation it uplifts.");

        // A pay cut is a legitimate correction, but a swing this large is far more likely a typo
        // than an intended award.
        RuleFor(x => x.DefaultPercentage)
            .InclusiveBetween(-100m, 100m)
            .WithMessage("The uplift must be between -100% and 100%.");

        RuleForEach(x => x.ScaleOverrides).ChildRules(o =>
        {
            o.RuleFor(x => x.PayScaleId)
                .NotEmpty().WithMessage("A pay scale is required.");

            o.RuleFor(x => x.Percentage)
                .InclusiveBetween(-100m, 100m)
                .WithMessage("The uplift must be between -100% and 100%.");
        });

        RuleFor(x => x.ScaleOverrides)
            .Must(o => o.Select(x => x.PayScaleId).Distinct().Count() == o.Count)
            .WithMessage("A pay scale can only be given one uplift.");
    }
}

