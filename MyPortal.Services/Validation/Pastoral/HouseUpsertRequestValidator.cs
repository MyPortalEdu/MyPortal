using FluentValidation;
using MyPortal.Contracts.Models.Pastoral;

namespace MyPortal.Services.Validation.Pastoral;

public class HouseUpsertRequestValidator : AbstractValidator<HouseUpsertRequest>
{
    public HouseUpsertRequestValidator()
    {
        RuleFor(x => x.AcademicYearId)
            .NotEqual(Guid.Empty).WithMessage("Academic year is required.");

        // Persists to StudentGroups: Code [Required] nvarchar(10), Description (Name) [Required]
        // nvarchar(256), Notes nvarchar(256).
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(10).WithMessage("Code must not exceed 10 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(256).WithMessage("Name must not exceed 256 characters.");

        RuleFor(x => x.Notes)
            .MaximumLength(256).WithMessage("Notes must not exceed 256 characters.");

        // Houses.ColourCode nvarchar(10).
        RuleFor(x => x.ColourCode)
            .MaximumLength(10).WithMessage("Colour code must not exceed 10 characters.");

        RuleForEach(x => x.Supervisors).SetValidator(new StudentGroupSupervisorUpsertRequestValidator());
    }
}
