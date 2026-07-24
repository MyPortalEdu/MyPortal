using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class StaffEqualityDetailsUpsertRequestValidator : AbstractValidator<StaffEqualityDetailsUpsertRequest>
{
    public StaffEqualityDetailsUpsertRequestValidator()
    {
        RuleFor(x => x.DisabilityDetails)
            .MaximumLength(1024).WithMessage("Disability details must not exceed 1024 characters.");

        RuleFor(x => x.DisabilityNumber)
            .MaximumLength(32).WithMessage("The disability number must not exceed 32 characters.");

        RuleFor(x => x.DeclaredDisabilities)
            .Must(rows => rows.Select(r => r.DisabilityId).Distinct().Count() == rows.Count)
            .WithMessage("The same disability cannot be selected more than once.");

        RuleForEach(x => x.DeclaredDisabilities).ChildRules(disability =>
        {
            disability.RuleFor(d => d.DisabilityId)
                .NotEmpty().WithMessage("A disability is required.");

            disability.RuleFor(d => d.AssistanceRequired)
                .MaximumLength(512).WithMessage("Assistance required must not exceed 512 characters.");
        });
    }
}
