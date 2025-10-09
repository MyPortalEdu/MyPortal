using FluentValidation;
using MyPortal.Auth.Constants;
using MyPortal.Contracts.Interfaces.Users;
using MyPortal.Contracts.Models.System.Users;

namespace MyPortal.Services.Validation;

public class UserValidators
{
    public class UserUpsertValidator<T> : AbstractValidator<T> where T : IUserUpsertDto
    {
        public UserUpsertValidator()
        {
            RuleFor(x => x.PersonId)
                .Must(id => id == null || id.Value != Guid.Empty)
                .WithMessage("PersonId cannot be an empty GUID");

            RuleFor(x => x.UserType).IsInEnum();

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MaximumLength(256).WithMessage("Username must not exceed 256 characters");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .Must(e => string.IsNullOrWhiteSpace(e) || e.Length <= 256)
                .WithMessage("Email must not exceed 256 characters")
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("Invalid email format");

            RuleFor(x => x.RoleIds)
                .NotNull().WithMessage("RoleIds cannot be null")
                .Must(list => list.Count == list.Distinct().Count())
                .WithMessage("RoleIds must be unique");

            RuleForEach(x => x.RoleIds)
                .NotEmpty().WithMessage("RoleId cannot be an empty GUID");
        }
    }

    public class UserPasswordValidator<T> : AbstractValidator<T> where T : IUserPasswordDto
    {
        public UserPasswordValidator()
        {
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(PasswordRequirements.RequiredLength)
                .WithMessage($"Password must be at least {PasswordRequirements.RequiredLength} characters long");

            if (PasswordRequirements.RequireUppercase)
                RuleFor(x => x.Password).Matches("[A-Z]")
                    .WithMessage("Password must contain at least one uppercase letter");

            if (PasswordRequirements.RequireLowercase)
                RuleFor(x => x.Password).Matches("[a-z]")
                    .WithMessage("Password must contain at least one lowercase letter");

            if (PasswordRequirements.RequireDigit)
                RuleFor(x => x.Password).Matches("[0-9]").WithMessage("Password must contain at least one digit");

            if (PasswordRequirements.RequireNonAlphanumeric)
                RuleFor(x => x.Password).Matches("[^a-zA-Z0-9]")
                    .WithMessage("Password must contain at least one non-alphanumeric character.");
        }
    }
    
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            Include(new UserUpsertValidator<CreateUserDto>());
            Include(new UserPasswordValidator<CreateUserDto>());
        }
    }

    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            Include(new UserUpsertValidator<UpdateUserDto>());
            Include(new GenericValidators.UpdateValidator<UpdateUserDto>());
        }
    }
}