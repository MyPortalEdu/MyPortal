using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class StaffEmploymentDetailsUpsertRequestValidator
    : AbstractValidator<StaffEmploymentDetailsUpsertRequest>
{
    public StaffEmploymentDetailsUpsertRequestValidator()
    {
        RuleFor(x => x.BankName)
            .MaximumLength(50).WithMessage("The bank name must not exceed 50 characters.");

        RuleFor(x => x.BankAccount)
            .MaximumLength(15).WithMessage("The bank account number must not exceed 15 characters.");

        RuleFor(x => x.BankSortCode)
            .MaximumLength(10).WithMessage("The sort code must not exceed 10 characters.");

        RuleFor(x => x.NiNumber)
            .MaximumLength(9).WithMessage("The National Insurance number must not exceed 9 characters.");

        RuleForEach(x => x.Employments).ChildRules(employment =>
        {
            employment.RuleFor(e => e.StartDate)
                .NotEmpty().WithMessage("An employment start date is required.");

            employment.RuleFor(e => e.EndDate)
                .GreaterThanOrEqualTo(e => e.StartDate)
                .When(e => e.EndDate.HasValue)
                .WithMessage("The employment end date cannot be before its start date.");

            employment.RuleFor(e => e.Notes)
                .MaximumLength(1024).WithMessage("Notes must not exceed 1024 characters.");

            employment.RuleForEach(e => e.Contracts).ChildRules(contract =>
            {
                contract.RuleFor(c => c.ContractTypeId)
                    .NotEmpty().WithMessage("A contract type is required.");

                contract.RuleFor(c => c.PostTitle)
                    .NotEmpty().WithMessage("A post title is required.")
                    .MaximumLength(256).WithMessage("The post title must not exceed 256 characters.");

                contract.RuleFor(c => c.SpinePoint)
                    .MaximumLength(20).WithMessage("The spine point must not exceed 20 characters.");

                contract.RuleFor(c => c.StartDate)
                    .NotEmpty().WithMessage("A contract start date is required.");

                contract.RuleFor(c => c.EndDate)
                    .GreaterThanOrEqualTo(c => c.StartDate)
                    .When(c => c.EndDate.HasValue)
                    .WithMessage("The contract end date cannot be before its start date.");

                contract.RuleFor(c => c.Fte)
                    .InclusiveBetween(0m, 1m).WithMessage("FTE must be between 0 and 1.");

                contract.RuleFor(c => c.HoursPerWeek)
                    .GreaterThanOrEqualTo(0m).When(c => c.HoursPerWeek.HasValue)
                    .WithMessage("Hours per week cannot be negative.");

                contract.RuleFor(c => c.AnnualSalary)
                    .GreaterThanOrEqualTo(0m).When(c => c.AnnualSalary.HasValue)
                    .WithMessage("The annual salary cannot be negative.");
            });
        });
    }
}
