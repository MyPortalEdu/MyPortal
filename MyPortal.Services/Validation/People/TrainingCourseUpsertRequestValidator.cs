using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class TrainingCourseUpsertRequestValidator : AbstractValidator<TrainingCourseUpsertRequest>
{
    public TrainingCourseUpsertRequestValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("A code is required.").MaximumLength(128);
        RuleFor(x => x.Name).NotEmpty().WithMessage("A name is required.").MaximumLength(128);
        RuleFor(x => x.Description).MaximumLength(256);
    }
}
