using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class SetSenStatusRequestValidator : AbstractValidator<SetSenStatusRequest>
{
    public SetSenStatusRequestValidator()
    {
        RuleFor(x => x.SenStatusId)
            .NotEmpty()
            .WithMessage("A SEN status must be selected.");

        RuleFor(x => x.StartDate)
            .NotEmpty()
            .WithMessage("A start date is required.");
    }
}
