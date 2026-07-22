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

        // Two employment spells for the same person must not overlap. An open-ended ("current")
        // spell has no EndDate and runs to infinity, so nothing may start on or after it.
        RuleFor(x => x.Employments)
            .Must(EmploymentsDoNotOverlap).WithMessage("Employment periods must not overlap.")
            .When(x => x.Employments.Count > 1);

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

            // Every contract must sit within its parent employment spell.
            employment.RuleFor(e => e)
                .Must(ContractsWithinEmployment)
                .WithMessage("Each contract must fall within its employment period.");

            employment.RuleForEach(e => e.Contracts).ChildRules(contract =>
            {
                contract.RuleFor(c => c.ContractTypeId)
                    .NotEmpty().WithMessage("A contract type is required.");

                contract.RuleFor(c => c.PostTitle)
                    .NotEmpty().WithMessage("A post title is required.")
                    .MaximumLength(256).WithMessage("The post title must not exceed 256 characters.");

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

                contract.RuleForEach(c => c.Allowances).ChildRules(allowance =>
                {
                    allowance.RuleFor(a => a.AdditionalPaymentTypeId)
                        .NotEmpty().WithMessage("An allowance type is required.");

                    allowance.RuleFor(a => a.Amount)
                        .GreaterThanOrEqualTo(0m).WithMessage("The allowance amount cannot be negative.");

                    allowance.RuleFor(a => a.PayFactor)
                        .InclusiveBetween(0m, 1m).When(a => a.PayFactor.HasValue)
                        .WithMessage("The pay factor must be between 0 and 1.");

                    allowance.RuleFor(a => a.StartDate)
                        .NotEmpty().WithMessage("An allowance start date is required.");

                    allowance.RuleFor(a => a.EndDate)
                        .GreaterThanOrEqualTo(a => a.StartDate)
                        .When(a => a.EndDate.HasValue)
                        .WithMessage("The allowance end date cannot be before its start date.");

                    allowance.RuleFor(a => a.Reason)
                        .MaximumLength(256).WithMessage("The reason must not exceed 256 characters.");
                });

                contract.RuleForEach(c => c.Suspensions).ChildRules(suspension =>
                {
                    suspension.RuleFor(s => s.StartDate)
                        .NotEmpty().WithMessage("A suspension start date is required.");

                    suspension.RuleFor(s => s.EndDate)
                        .GreaterThanOrEqualTo(s => s.StartDate)
                        .When(s => s.EndDate.HasValue)
                        .WithMessage("The suspension end date cannot be before its start date.");

                    suspension.RuleFor(s => s.Reason)
                        .MaximumLength(256).WithMessage("The reason must not exceed 256 characters.");
                });
            });
        });
    }

    private static bool EmploymentsDoNotOverlap(List<StaffEmploymentUpsertItem> employments)
    {
        var ordered = employments.OrderBy(e => e.StartDate).ToList();

        for (var i = 1; i < ordered.Count; i++)
        {
            var previousEnd = ordered[i - 1].EndDate ?? DateTime.MaxValue;

            if (ordered[i].StartDate <= previousEnd)
            {
                return false;
            }
        }

        return true;
    }

    private static bool ContractsWithinEmployment(StaffEmploymentUpsertItem employment)
    {
        foreach (var contract in employment.Contracts)
        {
            if (contract.StartDate < employment.StartDate)
            {
                return false;
            }

            if (employment.EndDate.HasValue)
            {
                if (contract.StartDate > employment.EndDate.Value)
                {
                    return false;
                }

                if (contract.EndDate.HasValue && contract.EndDate.Value > employment.EndDate.Value)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
