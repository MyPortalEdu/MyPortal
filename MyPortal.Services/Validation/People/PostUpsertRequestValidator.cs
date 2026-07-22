using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class PostUpsertRequestValidator : AbstractValidator<PostUpsertRequest>
{
    public PostUpsertRequestValidator()
    {
        RuleFor(x => x.Reference)
            .NotEmpty().WithMessage("A post reference is required.")
            .MaximumLength(32).WithMessage("The reference must not exceed 32 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A post title is required.")
            .MaximumLength(256).WithMessage("The title must not exceed 256 characters.");

        RuleFor(x => x.SwrPostCode)
            .MaximumLength(10).WithMessage("The SWR post code must not exceed 10 characters.");

        RuleFor(x => x.EstablishedFte)
            .InclusiveBetween(0m, 100m).When(x => x.EstablishedFte.HasValue)
            .WithMessage("The established FTE must be between 0 and 100.");

        RuleForEach(x => x.Vacancies).ChildRules(vacancy =>
        {
            vacancy.RuleFor(v => v.StartDate)
                .NotEmpty().WithMessage("A vacancy start date is required.");

            vacancy.RuleFor(v => v.EndDate)
                .GreaterThanOrEqualTo(v => v.StartDate)
                .When(v => v.EndDate.HasValue)
                .WithMessage("The vacancy end date cannot be before its start date.");

            vacancy.RuleFor(v => v.Notes)
                .MaximumLength(256).WithMessage("Notes must not exceed 256 characters.");
        });
    }
}
