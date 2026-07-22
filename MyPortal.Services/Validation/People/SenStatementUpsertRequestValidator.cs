using FluentValidation;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Students;

namespace MyPortal.Services.Validation.People;

public class SenStatementUpsertRequestValidator : AbstractValidator<SenStatementUpsertRequest>
{
    public SenStatementUpsertRequestValidator()
    {
        RuleFor(x => x.AssessmentRequestDate)
            .NotEmpty()
            .WithMessage("An assessment request date is required.");

        RuleFor(x => x.ParentConsultDate)
            .GreaterThanOrEqualTo(x => x.AssessmentRequestDate)
            .When(x => x.ParentConsultDate.HasValue)
            .WithMessage("The parent consultation date cannot be before the assessment request date.");

        RuleFor(x => x.FinalisedDate)
            .GreaterThanOrEqualTo(x => x.AssessmentRequestDate)
            .When(x => x.FinalisedDate.HasValue)
            .WithMessage("The finalised date cannot be before the assessment request date.");

        RuleFor(x => x.CeasedDate)
            .GreaterThanOrEqualTo(x => x.AssessmentRequestDate)
            .When(x => x.CeasedDate.HasValue)
            .WithMessage("The ceased date cannot be before the assessment request date.");

        RuleFor(x => x.CeasedDate)
            .GreaterThanOrEqualTo(x => x.FinalisedDate!.Value)
            .When(x => x.CeasedDate.HasValue && x.FinalisedDate.HasValue)
            .WithMessage("The ceased date cannot be before the finalised date.");

        RuleFor(x => x.AppealNotes)
            .MaximumLength(1024);

        RuleFor(x => x.TemporaryDisapplicationSubjects)
            .MaximumLength(256);

        RuleFor(x => x.PermanentDisapplicationSubjects)
            .MaximumLength(256);

        RuleFor(x => x.Comments)
            .MaximumLength(1024);
    }
}
