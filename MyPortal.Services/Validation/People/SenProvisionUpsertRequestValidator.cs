using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class SenProvisionUpsertRequestValidator : AbstractValidator<SenProvisionUpsertRequest>
{
    public SenProvisionUpsertRequestValidator()
    {
        RuleFor(x => x.SenProvisionTypeId)
            .NotEmpty()
            .WithMessage("A provision type must be selected.");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("A start date is required.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("The end date must be on or after the start date.");

        RuleFor(x => x.Note)
            .NotEmpty()
            .WithMessage("A note is required.");

        RuleFor(x => x.Frequency)
            .MaximumLength(128);

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Cost.HasValue)
            .WithMessage("Cost cannot be negative.");
    }
}
