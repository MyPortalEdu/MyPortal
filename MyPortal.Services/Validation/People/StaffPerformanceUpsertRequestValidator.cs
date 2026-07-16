using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class StaffPerformanceUpsertRequestValidator : AbstractValidator<StaffPerformanceUpsertRequest>
{
    public StaffPerformanceUpsertRequestValidator()
    {
        RuleForEach(x => x.Reviews).ChildRules(review =>
        {
            review.RuleFor(r => r.CycleName)
                .MaximumLength(128).WithMessage("The cycle name must not exceed 128 characters.");

            review.RuleFor(r => r.NextReviewDate)
                .GreaterThanOrEqualTo(r => r.ReviewDate!.Value)
                .When(r => r.ReviewDate.HasValue && r.NextReviewDate.HasValue)
                .WithMessage("The next review date cannot be before the review date.");
        });

        RuleForEach(x => x.Objectives).ChildRules(objective =>
        {
            objective.RuleFor(o => o.Title)
                .NotEmpty().WithMessage("An objective title is required.")
                .MaximumLength(256).WithMessage("The objective title must not exceed 256 characters.");
        });

        RuleForEach(x => x.Observations).ChildRules(observation =>
        {
            observation.RuleFor(o => o.Date)
                .NotEmpty().WithMessage("An observation date is required.");

            observation.RuleFor(o => o.ObserverId)
                .NotEmpty().WithMessage("An observer is required.");

            observation.RuleFor(o => o.OutcomeId)
                .NotEmpty().WithMessage("An observation outcome is required.");
        });

        RuleForEach(x => x.TrainingRecords).ChildRules(training =>
        {
            training.RuleFor(t => t.TrainingCourseId)
                .NotEmpty().WithMessage("A training course is required.");

            training.RuleFor(t => t.StatusId)
                .NotEmpty().WithMessage("A training status is required.");

            training.RuleFor(t => t.ExpiryDate)
                .GreaterThanOrEqualTo(t => t.CompletedDate!.Value)
                .When(t => t.CompletedDate.HasValue && t.ExpiryDate.HasValue)
                .WithMessage("The expiry date cannot be before the completion date.");
        });
    }
}
