using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class StaffProfessionalDetailsUpsertRequestValidator
    : AbstractValidator<StaffProfessionalDetailsUpsertRequest>
{
    public StaffProfessionalDetailsUpsertRequestValidator()
    {
        // DfE Teacher Reference Number — exactly 7 digits when supplied.
        RuleFor(x => x.TeacherReferenceNumber)
            .Matches("^[0-9]{7}$").WithMessage("The teacher reference number must be 7 digits.")
            .When(x => !string.IsNullOrWhiteSpace(x.TeacherReferenceNumber));

        RuleFor(x => x.QualificationsSummary)
            .MaximumLength(128).WithMessage("The qualifications summary must not exceed 128 characters.");

        RuleForEach(x => x.Qualifications).ChildRules(qualification =>
        {
            qualification.RuleFor(q => q.Title)
                .NotEmpty().WithMessage("A qualification title is required.")
                .MaximumLength(256).WithMessage("The title must not exceed 256 characters.");

            qualification.RuleFor(q => q.Subject)
                .MaximumLength(256).WithMessage("The subject must not exceed 256 characters.");

            qualification.RuleFor(q => q.AwardingBody)
                .MaximumLength(256).WithMessage("The awarding body must not exceed 256 characters.");

            qualification.RuleFor(q => q.Grade)
                .MaximumLength(20).WithMessage("The grade must not exceed 20 characters.");

            qualification.RuleFor(q => q.YearAwarded)
                .InclusiveBetween(1900, 2100).WithMessage("Enter a valid year awarded.")
                .When(q => q.YearAwarded.HasValue);
        });
    }
}
