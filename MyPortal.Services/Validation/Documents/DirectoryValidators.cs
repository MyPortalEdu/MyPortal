using FluentValidation;
using MyPortal.Common.Constants;
using MyPortal.Contracts.Models.Documents;

namespace MyPortal.Services.Validation.Documents
{
    public class DirectoryValidators
    {
        public class DirectoryUpsertRequestValidator : AbstractValidator<DirectoryUpsertRequest>
        {
            public DirectoryUpsertRequestValidator()
            {
                RuleFor(x => x.ParentId)
                    .Must(id => id != Guid.Empty)
                    .WithMessage("ParentId cannot be an empty GUID.");

                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Directory name is required.")
                    .MaximumLength(256).WithMessage("Directory name must not exceed 256 characters.")
                    .Matches(RegularExpressions.AllowedDirectoryNameChars)
                    .WithMessage("Directory name must not contain illegal characters.");
            }
        }
    }
}
