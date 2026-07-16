using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class StaffPreEmploymentChecksUpsertRequestValidator
    : AbstractValidator<StaffPreEmploymentChecksUpsertRequest>
{
    public StaffPreEmploymentChecksUpsertRequestValidator()
    {
        RuleForEach(x => x.DbsChecks).ChildRules(dbs =>
        {
            dbs.RuleFor(d => d.DbsCheckTypeId)
                .NotEmpty().WithMessage("A DBS check type is required.");

            dbs.RuleFor(d => d.CertificateNumber)
                .NotEmpty().WithMessage("A DBS certificate number is required.")
                .MaximumLength(20).WithMessage("The certificate number must not exceed 20 characters.");

            dbs.RuleFor(d => d.IssueDate)
                .NotEmpty().WithMessage("A DBS issue date is required.");

            dbs.RuleFor(d => d.ExpiryDate)
                .GreaterThanOrEqualTo(d => d.IssueDate)
                .When(d => d.ExpiryDate.HasValue)
                .WithMessage("The DBS expiry date cannot be before its issue date.");
        });

        RuleForEach(x => x.RightToWorkChecks).ChildRules(rtw =>
        {
            rtw.RuleFor(r => r.DocumentTypeId)
                .NotEmpty().WithMessage("A right-to-work document type is required.");

            rtw.RuleFor(r => r.CheckDate)
                .NotEmpty().WithMessage("A right-to-work check date is required.");

            rtw.RuleFor(r => r.DocumentNumber)
                .MaximumLength(64).WithMessage("The document number must not exceed 64 characters.");
        });

        RuleForEach(x => x.References).ChildRules(reference =>
        {
            reference.RuleFor(r => r.RefereeName)
                .NotEmpty().WithMessage("A referee name is required.")
                .MaximumLength(256).WithMessage("The referee name must not exceed 256 characters.");

            reference.RuleFor(r => r.RefereeOrganisation)
                .MaximumLength(256).WithMessage("The referee organisation must not exceed 256 characters.");

            reference.RuleFor(r => r.RefereeEmail)
                .MaximumLength(256).WithMessage("The referee email must not exceed 256 characters.")
                .EmailAddress().When(r => !string.IsNullOrWhiteSpace(r.RefereeEmail))
                .WithMessage("The referee email must be a valid email address.");

            reference.RuleFor(r => r.ReceivedDate)
                .GreaterThanOrEqualTo(r => r.RequestedDate!.Value)
                .When(r => r.RequestedDate.HasValue && r.ReceivedDate.HasValue)
                .WithMessage("The reference received date cannot be before the date it was requested.");
        });

        RuleForEach(x => x.OverseasChecks).ChildRules(overseas =>
        {
            overseas.RuleFor(o => o.NationalityId)
                .NotEmpty().WithMessage("A country is required for an overseas check.");
        });
    }
}
