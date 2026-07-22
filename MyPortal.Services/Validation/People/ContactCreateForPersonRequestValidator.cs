using FluentValidation;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Contacts;

namespace MyPortal.Services.Validation.People;

public class ContactCreateForPersonRequestValidator : AbstractValidator<ContactCreateForPersonRequest>
{
    public ContactCreateForPersonRequestValidator()
    {
        RuleFor(x => x.PersonId)
            .NotEmpty().WithMessage("A person must be selected.");
    }
}
