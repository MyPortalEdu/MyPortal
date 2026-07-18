using FluentValidation;
using MyPortal.Contracts.Models.Pastoral;

namespace MyPortal.Services.Validation.Pastoral;

public class RegGroupUpsertRequestValidator : AbstractValidator<RegGroupUpsertRequest>
{
    public RegGroupUpsertRequestValidator()
    {
        RuleFor(x => x.AcademicYearId)
            .NotEqual(Guid.Empty).WithMessage("Academic year is required.");

        RuleFor(x => x.YearGroupId)
            .NotEqual(Guid.Empty).WithMessage("Year group is required.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(10).WithMessage("Code must not exceed 10 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(256).WithMessage("Notes must not exceed 256 characters.");

        RuleForEach(x => x.Supervisors).SetValidator(new StudentGroupSupervisorUpsertRequestValidator());
    }
}
