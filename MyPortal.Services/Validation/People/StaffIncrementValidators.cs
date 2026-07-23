using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class IncrementPreviewRequestValidator : AbstractValidator<IncrementPreviewRequest>
{
    public IncrementPreviewRequestValidator()
    {
        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("An effective date is required.");
    }
}

public class IncrementApplyRequestValidator : AbstractValidator<IncrementApplyRequest>
{
    public IncrementApplyRequestValidator()
    {
        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("An effective date is required.");

        RuleFor(x => x.ContractIds)
            .NotNull();
    }
}

public class IncrementScheduleRequestValidator : AbstractValidator<IncrementScheduleRequest>
{
    public IncrementScheduleRequestValidator()
    {
        RuleFor(x => x.EffectiveFrom)
            .NotEmpty().WithMessage("An effective date is required.");
    }
}
