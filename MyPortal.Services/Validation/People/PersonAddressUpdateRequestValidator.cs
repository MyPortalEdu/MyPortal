using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class PersonAddressUpdateRequestValidator : AbstractValidator<PersonAddressUpdateRequest>
{
    public PersonAddressUpdateRequestValidator()
    {
        RuleFor(x => x.TypeId)
            .NotEmpty().WithMessage("Address type is required.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate!.Value)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("End date must be on or after the start date.");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.")
            .MaximumLength(256);
        RuleFor(x => x.Town)
            .NotEmpty().WithMessage("Town is required.")
            .MaximumLength(256);
        RuleFor(x => x.County)
            .NotEmpty().WithMessage("County is required.")
            .MaximumLength(256);
        RuleFor(x => x.Postcode)
            .NotEmpty().WithMessage("Postcode is required.")
            .MaximumLength(128);
        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(128);

        RuleFor(x => x.BuildingNumber).MaximumLength(128);
        RuleFor(x => x.BuildingName).MaximumLength(128);
        RuleFor(x => x.Apartment).MaximumLength(128);
        RuleFor(x => x.District).MaximumLength(256);
    }
}
