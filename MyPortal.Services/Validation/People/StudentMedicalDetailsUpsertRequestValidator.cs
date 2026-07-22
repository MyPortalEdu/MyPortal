using FluentValidation;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Students;

namespace MyPortal.Services.Validation.People;

public class StudentMedicalDetailsUpsertRequestValidator : AbstractValidator<StudentMedicalDetailsUpsertRequest>
{
    public StudentMedicalDetailsUpsertRequestValidator()
    {
        RuleForEach(x => x.Conditions).ChildRules(condition =>
        {
            condition.RuleFor(c => c.MedicalConditionId)
                .NotEmpty()
                .WithMessage("A medical condition must be selected.");

            condition.RuleFor(c => c.Medication)
                .MaximumLength(256);

            // Medication detail is meaningful only when the condition requires it.
            condition.RuleFor(c => c.Medication)
                .NotEmpty()
                .When(c => c.RequiresMedication)
                .WithMessage("Enter the medication, or clear 'requires medication'.");

            condition.RuleFor(c => c.Medication)
                .Empty()
                .When(c => !c.RequiresMedication)
                .WithMessage("Medication can only be set when the condition requires medication.");

            condition.RuleFor(c => c.EndDate)
                .GreaterThanOrEqualTo(c => c.StartDate!.Value)
                .When(c => c.StartDate.HasValue && c.EndDate.HasValue)
                .WithMessage("The resolved date must be on or after the start date.");
        });
    }
}
