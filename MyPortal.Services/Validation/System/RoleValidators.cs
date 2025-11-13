using FluentValidation;
using MyPortal.Contracts.Models.System.Roles;

namespace MyPortal.Services.Validation.System
{
    public class RoleValidators
    {
        public class UpsertRoleDtoValidator : AbstractValidator<RoleUpsertRequest>
        {
            public UpsertRoleDtoValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Name is required")
                    .MaximumLength(256).WithMessage("Role name must not exceed 256 characters.");

                RuleFor(x => x.Description)
                    .MaximumLength(512).WithMessage("Description must not exceed 512 characters.")
                    .When(x => !string.IsNullOrWhiteSpace(x.Description));

                RuleFor(x => x.PermissionIds)
                    .NotNull().WithMessage("PermissionIds cannot be null")
                    .Must(list => list.Count == list.Distinct().Count())
                    .WithMessage("PermissionIds must be unique.");

                RuleForEach(x => x.PermissionIds)
                    .NotEmpty().WithMessage("PermissionId cannot be an empty GUID.");
            }
        }
    }
}
