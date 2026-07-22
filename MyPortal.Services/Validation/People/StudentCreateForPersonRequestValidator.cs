using FluentValidation;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Students;

namespace MyPortal.Services.Validation.People;

public class StudentCreateForPersonRequestValidator : AbstractValidator<StudentCreateForPersonRequest>
{
    public StudentCreateForPersonRequestValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("A person must be selected.");
    }
}
