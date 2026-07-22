using FluentValidation;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Students;

namespace MyPortal.Services.Validation.People;

public class StudentCulturalDetailsUpsertRequestValidator : AbstractValidator<StudentCulturalDetailsUpsertRequest>
{
    public StudentCulturalDetailsUpsertRequestValidator()
    {
        RuleFor(x => x.EnglishProficiencyDate)
            .LessThanOrEqualTo(_ => DateTime.UtcNow.Date.AddDays(1))
            .When(x => x.EnglishProficiencyDate.HasValue)
            .WithMessage("The English proficiency assessment date cannot be in the future.");
    }
}
