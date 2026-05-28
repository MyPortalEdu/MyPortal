using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class StaffMemberUpsertRequestValidator : AbstractValidator<StaffMemberUpsertRequest>
{
    public StaffMemberUpsertRequestValidator()
    {
        // Person bio is validated by its own validator (name/gender/length rules).
        RuleFor(x => x.Person)
            .NotNull().WithMessage("Person details are required.")
            .SetValidator(new PersonUpsertRequestValidator());

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Staff code is required.")
            .MaximumLength(128).WithMessage("Staff code must not exceed 128 characters.");

        RuleFor(x => x.BankName)
            .MaximumLength(50).WithMessage("Bank name must not exceed 50 characters.");

        RuleFor(x => x.BankAccount)
            .MaximumLength(15).WithMessage("Bank account must not exceed 15 characters.");

        RuleFor(x => x.BankSortCode)
            .MaximumLength(10).WithMessage("Bank sort code must not exceed 10 characters.");

        RuleFor(x => x.NiNumber)
            .MaximumLength(9).WithMessage("NI number must not exceed 9 characters.");

        // DfE Teacher Reference Number is 7 digits.
        RuleFor(x => x.TeacherReferenceNumber)
            .MaximumLength(7).WithMessage("Teacher reference number must not exceed 7 characters.");

        RuleFor(x => x.Qualifications)
            .MaximumLength(128).WithMessage("Qualifications must not exceed 128 characters.");

        RuleFor(x => x.PpaPeriodsPerWeek)
            .GreaterThanOrEqualTo(0).WithMessage("PPA periods per week must be zero or greater.");
    }
}
