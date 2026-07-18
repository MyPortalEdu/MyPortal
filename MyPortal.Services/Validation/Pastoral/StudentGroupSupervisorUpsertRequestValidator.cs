using FluentValidation;
using MyPortal.Contracts.Models.Pastoral;

namespace MyPortal.Services.Validation.Pastoral;

public class StudentGroupSupervisorUpsertRequestValidator : AbstractValidator<StudentGroupSupervisorUpsertRequest>
{
    public StudentGroupSupervisorUpsertRequestValidator()
    {
        RuleFor(x => x.StaffMemberId)
            .NotEqual(Guid.Empty).WithMessage("Supervisor staff member is required.");

        // StudentGroupSupervisors.Title is [Required] nvarchar(256).
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Supervisor title is required.")
            .MaximumLength(256).WithMessage("Supervisor title must not exceed 256 characters.");
    }
}
