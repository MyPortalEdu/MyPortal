using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class StaffEqualityDetailsUpsertRequestValidator : AbstractValidator<StaffEqualityDetailsUpsertRequest>
{
    public StaffEqualityDetailsUpsertRequestValidator()
    {
        RuleFor(x => x.DisabilityDetails)
            .MaximumLength(1024).WithMessage("Disability details must not exceed 1024 characters.");

        RuleFor(x => x.DisabilityIds)
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("The same disability cannot be selected more than once.");
    }
}
