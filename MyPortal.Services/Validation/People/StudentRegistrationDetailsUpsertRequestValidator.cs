using FluentValidation;
using MyPortal.Common.Identifiers;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class StudentRegistrationDetailsUpsertRequestValidator
    : AbstractValidator<StudentRegistrationDetailsUpsertRequest>
{
    public StudentRegistrationDetailsUpsertRequestValidator()
    {
        RuleFor(x => x.Upn)
            .MaximumLength(13).WithMessage("UPN must not exceed 13 characters.")
            .Must(BeAValidUpn).WithMessage("UPN is not valid — check the format and check letter.");

        RuleFor(x => x.FormerUpn)
            .MaximumLength(13).WithMessage("Former UPN must not exceed 13 characters.")
            .Must(BeAValidUpn).WithMessage("Former UPN is not valid — check the format and check letter.");

        RuleFor(x => x.Uln)
            .MaximumLength(10).WithMessage("ULN must not exceed 10 characters.");

        RuleFor(x => x.LaChildId)
            .MaximumLength(20).WithMessage("LA child ID must not exceed 20 characters.");
    }

    // UPN is optional; when supplied it must satisfy the DfE format + check letter.
    private static bool BeAValidUpn(string? upn)
        => string.IsNullOrWhiteSpace(upn) || Upn.IsValid(upn.Trim());
}
