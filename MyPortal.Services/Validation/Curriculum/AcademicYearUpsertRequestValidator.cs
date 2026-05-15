using FluentValidation;
using MyPortal.Contracts.Models.Curriculum;

namespace MyPortal.Services.Validation.Curriculum;

public class AcademicYearUpsertRequestValidator : AbstractValidator<AcademicYearUpsertRequest>
{
    public AcademicYearUpsertRequestValidator()
    {
        RuleFor(x => x.TimetableCycleLength)
            .GreaterThan(0).WithMessage("Timetable cycle length must be at least 1.");

        RuleFor(x => x.SchoolWeekLength)
            .GreaterThan(0).WithMessage("School week length must be at least 1.");

        // The cycle math advances by SchoolWeekLength days per calendar week, modulo
        // TimetableCycleLength — if the cycle isn't a whole number of school weeks the
        // offsets drift instead of repeating, which means a 5-day school with a 6-day cycle
        // would get the rotating-day pattern that a 6-day school expects.
        RuleFor(x => x)
            .Must(m => m.TimetableCycleLength % m.SchoolWeekLength == 0)
            .WithMessage("Timetable cycle length must be a multiple of the school week length.")
            .When(m => m.TimetableCycleLength > 0 && m.SchoolWeekLength > 0);

        // FirstWeekOffset is a 0-based index into the cycle; an offset >= length would point off
        // the end of the cycle and break week-A/week-B style scheduling.
        RuleFor(x => x.FirstWeekOffset)
            .GreaterThanOrEqualTo(0).WithMessage("First week offset must be zero or greater.")
            .Must((model, offset) => offset < model.TimetableCycleLength)
            .WithMessage("First week offset must be less than the timetable cycle length.");

        RuleFor(x => x.AcademicTerms)
            .NotEmpty().WithMessage("At least one academic term is required.")
            .Must(NotOverlap!).WithMessage("Academic terms must not overlap.")
            .When(x => x.AcademicTerms is not null);

        RuleForEach(x => x.AcademicTerms).SetValidator(new AcademicTermUpsertRequestValidator());

        RuleForEach(x => x.SchoolHolidays).SetValidator(new SchoolHolidayUpsertRequestValidator());

        // Periods are required, but the caller supplies them in one of two ways: by copying from
        // a prior year, or by listing them inline. Exactly one of those must be set — both is
        // ambiguous (the service silently picks copy), neither leaves the year unschedulable.
        RuleFor(x => x)
            .Must(x => x.CopyPeriodsFromAcademicYearId.HasValue || x.AttendancePeriods.Length > 0)
            .WithMessage("Either CopyPeriodsFromAcademicYearId or AttendancePeriods must be specified.");

        RuleFor(x => x)
            .Must(x => !(x.CopyPeriodsFromAcademicYearId.HasValue && x.AttendancePeriods.Length > 0))
            .WithMessage("Either CopyPeriodsFromAcademicYearId or AttendancePeriods must be specified.");

        RuleForEach(x => x.AttendancePeriods).SetValidator(new AttendancePeriodUpsertRequestValidator());

        // CycleDayIndex is bounded by the parent cycle length, so the cross-field check has to
        // live up here (the per-period validator can't see the cycle length).
        RuleFor(x => x.AttendancePeriods)
            .Must((model, periods) => periods.All(p 
                => p.CycleDayIndex < model.TimetableCycleLength))
            .WithMessage("Every attendance period's CycleDayIndex must be less than the timetable cycle length.")
            .When(x => x.TimetableCycleLength > 0);
    }

    private static bool NotOverlap(AcademicTermUpsertRequest[] terms)
    {
        var ordered = terms.OrderBy(t => t.StartDate).ToArray();
        for (var i = 1; i < ordered.Length; i++)
        {
            if (ordered[i].StartDate <= ordered[i - 1].EndDate)
            {
                return false;
            }
        }
        return true;
    }
}

public class AcademicTermUpsertRequestValidator : AbstractValidator<AcademicTermUpsertRequest>
{
    public AcademicTermUpsertRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Term name is required.")
            .MaximumLength(128).WithMessage("Term name must not exceed 128 characters.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("Term end date must be after the start date.");
    }
}

public class SchoolHolidayUpsertRequestValidator : AbstractValidator<SchoolHolidayUpsertRequest>
{
    public SchoolHolidayUpsertRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Holiday name is required.")
            .MaximumLength(128).WithMessage("Holiday name must not exceed 128 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Holiday type is invalid.");

        // Single-day holidays are valid (Start == End), so this is GreaterThanOrEqualTo not GreaterThan.
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("Holiday end date must be on or after the start date.");
    }
}

public class AttendancePeriodUpsertRequestValidator : AbstractValidator<AttendancePeriodUpsertRequest>
{
    public AttendancePeriodUpsertRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Period name is required.")
            .MaximumLength(128).WithMessage("Period name must not exceed 128 characters.");

        RuleFor(x => x.CycleDayIndex)
            .GreaterThanOrEqualTo(0).WithMessage("CycleDayIndex must be zero or greater.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("Period end time must be after the start time.");

        // Mirrors the DB CHECK constraint — a period that is neither a lesson nor a
        // registration session would materialise to nothing.
        RuleFor(x => x)
            .Must(x => x.IsLesson || x.IsAmReg || x.IsPmReg)
            .WithMessage("A period must be a lesson, an AM reg, a PM reg, or some combination.");
    }
}
