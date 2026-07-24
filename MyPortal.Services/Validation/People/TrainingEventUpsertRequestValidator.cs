using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class TrainingEventUpsertRequestValidator : AbstractValidator<TrainingEventUpsertRequest>
{
    public TrainingEventUpsertRequestValidator()
    {
        RuleFor(x => x.TrainingCourseId).NotEmpty().WithMessage("A course is required.");

        RuleFor(x => x.Title).NotEmpty().WithMessage("A title is required.")
            .MaximumLength(200);

        RuleFor(x => x.StartTime).NotEmpty().WithMessage("A start date/time is required.");

        RuleFor(x => x.EndTime).GreaterThanOrEqualTo(x => x.StartTime)
            .When(x => x.EndTime.HasValue)
            .WithMessage("End must be on or after the start.");

        RuleFor(x => x.Hours).GreaterThanOrEqualTo(0).When(x => x.Hours.HasValue);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(0).When(x => x.Capacity.HasValue);
    }
}
