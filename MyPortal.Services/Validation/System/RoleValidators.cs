using FluentValidation;
using MyPortal.Contracts.Interfaces.System.Roles;
using MyPortal.Contracts.Models.System.Roles;

namespace MyPortal.Services.Validation.System
{
    public class RoleValidators
    {
        public class RoleUpsertValidator<T> : AbstractValidator<T> where T : IUpsertRoleDto
        {
            public RoleUpsertValidator()
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

        public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
        {
            public CreateRoleDtoValidator()
            {
                Include(new RoleUpsertValidator<CreateRoleDto>());
            }
        }

        public class UpdateRoleDtoValidator : AbstractValidator<UpdateRoleDto>
        {
            public UpdateRoleDtoValidator()
            {
                Include(new RoleUpsertValidator<UpdateRoleDto>());
                Include(new GenericValidators.UpdateValidator<UpdateRoleDto>());
            }
        }
    }
}
