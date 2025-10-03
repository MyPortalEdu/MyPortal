using FluentValidation;
using MyPortal.Contracts.Interfaces;

namespace MyPortal.Services.Validation;

public class GenericValidators
{
    public class UpdateValidator<T> : AbstractValidator<T> where T : IUpdateDto
    {
        public UpdateValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required")
                .Must(id => id != Guid.Empty).WithMessage("Id cannot be an empty GUID");
        }
    }
}