using FluentValidation;
using Microsoft.Extensions.Options;
using MyPortal.Auth.Constants;
using MyPortal.Common.Options;
using MyPortal.Contracts.Interfaces.System.Users;
using MyPortal.Contracts.Models.System.Users;

namespace MyPortal.Services.Validation.System;

public class UserValidators
{
    public class UserPasswordValidator<T> : AbstractValidator<T> where T : IUserPasswordRequest
    {
        public UserPasswordValidator(IOptions<PasswordOptions> passwordOptions)
        {
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(passwordOptions.Value.RequiredLength)
                .WithMessage($"Password must be at least {passwordOptions.Value.RequiredLength} characters long.");

            if (passwordOptions.Value.RequireUppercase)
                RuleFor(x => x.Password).Matches("[A-Z]")
                    .WithMessage("Password must contain at least one uppercase letter.");

            if (passwordOptions.Value.RequireLowercase)
                RuleFor(x => x.Password).Matches("[a-z]")
                    .WithMessage("Password must contain at least one lowercase letter.");

            if (passwordOptions.Value.RequireDigit)
                RuleFor(x => x.Password).Matches("[0-9]").WithMessage("Password must contain at least one digit.");

            if (passwordOptions.Value.RequireNonAlphanumeric)
                RuleFor(x => x.Password).Matches("[^a-zA-Z0-9]")
                    .WithMessage("Password must contain at least one non-alphanumeric character.");
        }
    }
    
    public class UpsertUserDtoValidator : AbstractValidator<UserUpsertRequest>
    {
        public UpsertUserDtoValidator(IOptions<PasswordOptions> passwordOptions)
        {
            Include(new UserPasswordValidator<UserUpsertRequest>(passwordOptions));
            
            RuleFor(x => x.PersonId)
                .Must(id => id == null || id.Value != Guid.Empty)
                .WithMessage("PersonId cannot be an empty GUID.");

            RuleFor(x => x.UserType).IsInEnum();

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MaximumLength(256).WithMessage("Username must not exceed 256 characters.");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .Must(e => string.IsNullOrWhiteSpace(e) || e.Length <= 256)
                .WithMessage("Email must not exceed 256 characters.")
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.Email))
                .WithMessage("Invalid email format.");

            RuleFor(x => x.RoleIds)
                .NotNull().WithMessage("RoleIds cannot be null")
                .Must(list => list.Count == list.Distinct().Count())
                .WithMessage("RoleIds must be unique.");

            RuleForEach(x => x.RoleIds)
                .NotEmpty().WithMessage("RoleId cannot be an empty GUID.");
        }
    }
}