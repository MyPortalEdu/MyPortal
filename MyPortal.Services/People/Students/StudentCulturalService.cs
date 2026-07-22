using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Students;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People.Students;

public class StudentCulturalService(
    IAuthorizationService authorizationService,
    ILogger<StudentCulturalService> logger,
    IStudentRepository studentRepository,
    IPersonRepository personRepository,
    IEthnicityRepository ethnicityRepository,
    ILanguageRepository languageRepository,
    IReligionRepository religionRepository,
    INationalityRepository nationalityRepository,
    IEnglishProficiencyRepository englishProficiencyRepository,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IStudentCulturalService
{
    public async Task<StudentCulturalDetailsResponse> GetCulturalDetailsAsync(Guid studentId,
        CancellationToken cancellationToken)
    {
        var (student, person) = await LoadAsync(studentId, cancellationToken);

        var ethnicities = await ethnicityRepository.GetListAsync(cancellationToken: cancellationToken);
        var languages = await languageRepository.GetListAsync(cancellationToken: cancellationToken);
        var religions = await religionRepository.GetListAsync(cancellationToken: cancellationToken);
        var nationalities = await nationalityRepository.GetListAsync(cancellationToken: cancellationToken);
        var proficiencies = await englishProficiencyRepository.GetListAsync(cancellationToken: cancellationToken);

        return new StudentCulturalDetailsResponse
        {
            EthnicityId = person.EthnicityId,
            FirstLanguageId = person.FirstLanguageId,
            ReligionId = person.ReligionId,
            NationalityId = person.NationalityId,
            EnglishProficiencyId = student.EnglishProficiencyId,
            EnglishProficiencyDate = student.EnglishProficiencyDate,
            Ethnicities = ethnicities.ToOrderedLookup(),
            Languages = languages.ToAlphabeticalLookup(),
            Religions = religions.ToOrderedLookup(),
            Nationalities = nationalities.ToAlphabeticalLookup(),
            EnglishProficiencies = proficiencies.ToOrderedLookup()
        };
    }

    public async Task UpdateCulturalDetailsAsync(Guid studentId, StudentCulturalDetailsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(model);

        var (student, person) = await LoadAsync(studentId, cancellationToken);

        // Only the cultural FKs are touched; the rest of the Person row (name, NHS no., marital
        // status, gender identity, ...) is preserved by load-modify-save.
        person.EthnicityId = model.EthnicityId;
        person.FirstLanguageId = model.FirstLanguageId;
        person.ReligionId = model.ReligionId;
        person.NationalityId = model.NationalityId;

        student.EnglishProficiencyId = model.EnglishProficiencyId;
        student.EnglishProficiencyDate = model.EnglishProficiencyDate;

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await personRepository.UpdateAsync(person, cancellationToken, uow.Transaction);
            await studentRepository.UpdateAsync(student, cancellationToken, uow.Transaction);
        }, cancellationToken);
    }

    private async Task<(Core.Entities.Student Student, Core.Entities.Person Person)> LoadAsync(Guid studentId,
        CancellationToken cancellationToken)
    {
        var student = await studentRepository.GetByIdAsync(studentId, cancellationToken);

        if (student == null)
        {
            throw new NotFoundException("Student not found.");
        }

        var person = await personRepository.GetByIdAsync(student.PersonId, cancellationToken);

        if (person == null)
        {
            throw new NotFoundException("Student not found.");
        }

        return (student, person);
    }
}
