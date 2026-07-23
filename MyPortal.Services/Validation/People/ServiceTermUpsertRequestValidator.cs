using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class ServiceTermUpsertRequestValidator : AbstractValidator<ServiceTermUpsertRequest>
{
    public ServiceTermUpsertRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("A code is required.")
            .MaximumLength(16).WithMessage("The code must not exceed 16 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A description is required.")
            .MaximumLength(256).WithMessage("The description must not exceed 256 characters.");

        RuleFor(x => x.IncrementMonth)
            .InclusiveBetween((byte)1, (byte)12).When(x => x.IncrementMonth.HasValue)
            .WithMessage("The increment month must be between 1 and 12.");

        RuleFor(x => x.IncrementDay)
            .InclusiveBetween((byte)1, (byte)31).When(x => x.IncrementDay.HasValue)
            .WithMessage("The increment day must be between 1 and 31.");

        RuleFor(x => x.IncrementMonth)
            .NotNull().When(x => x.SpinalProgression)
            .WithMessage("An increment month is required when spinal progression is enabled.");

        RuleFor(x => x.HoursPerWeek)
            .InclusiveBetween(0m, 168m).When(x => x.HoursPerWeek.HasValue)
            .WithMessage("Hours per week must be between 0 and 168.");

        RuleFor(x => x.WeeksPerYear)
            .InclusiveBetween(0m, 52.14m).When(x => x.WeeksPerYear.HasValue)
            .WithMessage("Weeks per year must be between 0 and 52.14.");

        RuleFor(x => x.SuperannuationSchemes)
            .Must(s => s.Select(x => x.SuperannuationSchemeId).Distinct().Count() == s.Count)
            .WithMessage("The same pension scheme cannot be added twice.");

        RuleFor(x => x.SuperannuationSchemes)
            .Must(s => s.Count(x => x.IsMain) <= 1)
            .WithMessage("Only one pension scheme can be marked as the main scheme.");
    }
}
