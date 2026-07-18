using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class StaffAbsencesUpsertRequestValidator : AbstractValidator<StaffAbsencesUpsertRequest>
{
    public StaffAbsencesUpsertRequestValidator()
    {
        // Absence periods for the same staff member must not overlap.
        RuleFor(x => x.Absences)
            .Must(AbsencesDoNotOverlap).WithMessage("Absence periods must not overlap.")
            .When(x => x.Absences.Count > 1);

        RuleForEach(x => x.Absences).ChildRules(absence =>
        {
            absence.RuleFor(a => a.AbsenceTypeId)
                .NotEmpty().WithMessage("An absence type is required.");

            absence.RuleFor(a => a.StartDate)
                .NotEmpty().WithMessage("An absence start date is required.");

            absence.RuleFor(a => a.EndDate)
                .NotEmpty().WithMessage("An absence end date is required.")
                .GreaterThanOrEqualTo(a => a.StartDate)
                .WithMessage("The absence end date cannot be before its start date.");

            absence.RuleFor(a => a.Notes)
                .MaximumLength(256).WithMessage("Notes must not exceed 256 characters.");
        });
    }

    private static bool AbsencesDoNotOverlap(List<StaffAbsenceUpsertItem> absences)
    {
        var ordered = absences.OrderBy(a => a.StartDate).ToList();

        for (var i = 1; i < ordered.Count; i++)
        {
            if (ordered[i].StartDate <= ordered[i - 1].EndDate)
            {
                return false;
            }
        }

        return true;
    }
}
