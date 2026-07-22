using FluentValidation;
using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Validation.People;

public class StudentContactRelationshipUpsertRequestValidator
    : AbstractValidator<StudentContactRelationshipUpsertRequest>
{
    public StudentContactRelationshipUpsertRequestValidator()
    {
        RuleFor(x => x.ContactId)
            .NotEmpty().WithMessage("A contact must be selected.");

        RuleFor(x => x.RelationshipTypeId)
            .NotEmpty().WithMessage("A relationship type is required.");

        RuleFor(x => x.ContactOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Contact priority must be zero or greater.");
    }
}
