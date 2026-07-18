using FluentValidation;
using MyPortal.Contracts.Models.School;

namespace MyPortal.Services.Validation.School;

public class SchoolValidator : AbstractValidator<SchoolUpsertRequest>
{
    public SchoolValidator()
    {
        // Name and Website persist to the school's backing Agency record (Agency.Name nvarchar(256),
        // Agency.Website [Url] nvarchar(100)).
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("School name is required.")
            .MaximumLength(256).WithMessage("School name must not exceed 256 characters.");

        RuleFor(x => x.Website)
            .MaximumLength(100).WithMessage("Website must not exceed 100 characters.")
            .Must(w => Uri.IsWellFormedUriString(w, UriKind.Absolute)).WithMessage("Website must be a valid URL.")
            .When(x => !string.IsNullOrWhiteSpace(x.Website));

        RuleFor(x => x.Urn)
            .NotEmpty().WithMessage("URN is required.")
            .MaximumLength(128).WithMessage("URN must not exceed 128 characters.");

        RuleFor(x => x.Uprn)
            .NotEmpty().WithMessage("UPRN is required.")
            .MaximumLength(128).WithMessage("UPRN must not exceed 128 characters.");

        // UK Provider Reference Number is an 8-digit number (nvarchar(8)).
        RuleFor(x => x.Ukprn)
            .Matches("^[0-9]{8}$").WithMessage("UKPRN must be exactly 8 digits.")
            .When(x => !string.IsNullOrWhiteSpace(x.Ukprn));

        RuleFor(x => x.EstablishmentNumber)
            .GreaterThanOrEqualTo(0).WithMessage("Establishment number must be zero or greater.");

        RuleFor(x => x.SchoolPhaseId).NotEqual(Guid.Empty).WithMessage("School phase is required.");
        RuleFor(x => x.SchoolTypeId).NotEqual(Guid.Empty).WithMessage("School type is required.");
        RuleFor(x => x.GovernanceTypeId).NotEqual(Guid.Empty).WithMessage("Governance type is required.");
        RuleFor(x => x.IntakeTypeId).NotEqual(Guid.Empty).WithMessage("Intake type is required.");

        RuleFor(x => x.LowestAge)
            .GreaterThanOrEqualTo(0).WithMessage("Lowest age must be zero or greater.")
            .When(x => x.LowestAge.HasValue);

        RuleFor(x => x.HighestAge)
            .GreaterThanOrEqualTo(0).WithMessage("Highest age must be zero or greater.")
            .When(x => x.HighestAge.HasValue);

        // Statutory age range must be ordered when both bounds are supplied.
        RuleFor(x => x)
            .Must(x => x.LowestAge <= x.HighestAge)
            .WithName(nameof(SchoolUpsertRequest.HighestAge))
            .WithMessage("Highest age must be greater than or equal to the lowest age.")
            .When(x => x.LowestAge.HasValue && x.HighestAge.HasValue);

        RuleFor(x => x.NetCapacity)
            .GreaterThanOrEqualTo(0).WithMessage("Net capacity must be zero or greater.")
            .When(x => x.NetCapacity.HasValue);

        RuleFor(x => x.MaxBoarders)
            .GreaterThanOrEqualTo(0).WithMessage("Maximum boarders must be zero or greater.")
            .When(x => x.MaxBoarders.HasValue);

        // Special-school establishment facts become mandatory once the school is flagged special.
        RuleFor(x => x.SpecialSchoolOrganisationId)
            .NotNull().WithMessage("Special school organisation is required for a special school.")
            .When(x => x.IsSpecialSchool);

        RuleFor(x => x.SpecialSchoolTypeId)
            .NotNull().WithMessage("Special school type is required for a special school.")
            .When(x => x.IsSpecialSchool);

        RuleFor(x => x.Telephone)
            .MaximumLength(30).WithMessage("Telephone must not exceed 30 characters.");

        RuleFor(x => x.Email)
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
