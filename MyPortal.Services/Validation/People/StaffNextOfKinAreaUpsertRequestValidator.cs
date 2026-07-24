using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class StaffNextOfKinAreaUpsertRequestValidator : AbstractValidator<StaffNextOfKinAreaUpsertRequest>
{
    public StaffNextOfKinAreaUpsertRequestValidator()
    {
        RuleForEach(x => x.Contacts).ChildRules(contact =>
        {
            contact.RuleFor(c => c.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(256).WithMessage("First name must not exceed 256 characters.");

            contact.RuleFor(c => c.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(256).WithMessage("Last name must not exceed 256 characters.");

            contact.RuleFor(c => c.MiddleName)
                .MaximumLength(256).WithMessage("Middle name must not exceed 256 characters.");

            contact.RuleFor(c => c.Title)
                .MaximumLength(128).WithMessage("Title must not exceed 128 characters.");

            contact.RuleFor(c => c.ContactOrder)
                .GreaterThanOrEqualTo(0).WithMessage("Call order must be zero or greater.");

            contact.RuleForEach(c => c.Emails).ChildRules(email =>
            {
                email.RuleFor(e => e.TypeId)
                    .NotEmpty().WithMessage("Email type is required.");

                email.RuleFor(e => e.Address)
                    .NotEmpty().WithMessage("Email address is required.")
                    .EmailAddress().WithMessage("Enter a valid email address.")
                    .MaximumLength(128).WithMessage("Email address must not exceed 128 characters.");
            });

            contact.RuleForEach(c => c.Phones).ChildRules(phone =>
            {
                phone.RuleFor(p => p.TypeId)
                    .NotEmpty().WithMessage("Phone type is required.");

                phone.RuleFor(p => p.Number)
                    .NotEmpty().WithMessage("Phone number is required.")
                    .MaximumLength(128).WithMessage("Phone number must not exceed 128 characters.");
            });

            // At most one main per list — the UI enforces this too, this is the guard.
            contact.RuleFor(c => c.Emails)
                .Must(emails => emails.Count(e => e.IsMain) <= 1)
                .WithMessage("Only one email can be marked as the main one.");

            contact.RuleFor(c => c.Phones)
                .Must(phones => phones.Count(p => p.IsMain) <= 1)
                .WithMessage("Only one phone number can be marked as the main one.");
        });
    }
}
