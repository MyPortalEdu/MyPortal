using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces.People;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

/// <summary>
/// Shared person-level equality mechanics, keyed by personId and auth-free — the calling subtype
/// service owns the access gate. See <see cref="IPersonEqualityService"/>.
/// </summary>
public class PersonEqualityService(
    IPersonRepository personRepository,
    IEthnicityRepository ethnicityRepository,
    INationalityRepository nationalityRepository,
    ILanguageRepository languageRepository,
    IMaritalStatusRepository maritalStatusRepository,
    IReligionRepository religionRepository,
    ISexualOrientationRepository sexualOrientationRepository,
    IGenderIdentityRepository genderIdentityRepository,
    IUnitOfWorkFactory unitOfWorkFactory)
    : IPersonEqualityService
{
    public async Task<StaffEqualityDetailsResponse> GetEqualityDetailsAsync(Guid personId,
        CancellationToken cancellationToken)
    {
        var person = await personRepository.GetByIdAsync(personId, cancellationToken);

        if (person == null)
        {
            throw new NotFoundException("Person not found.");
        }

        var ethnicities = await ethnicityRepository.GetListAsync(cancellationToken: cancellationToken);
        var nationalities = await nationalityRepository.GetListAsync(cancellationToken: cancellationToken);
        var languages = await languageRepository.GetListAsync(cancellationToken: cancellationToken);
        var maritalStatuses = await maritalStatusRepository.GetListAsync(cancellationToken: cancellationToken);
        var religions = await religionRepository.GetListAsync(cancellationToken: cancellationToken);
        var sexualOrientations = await sexualOrientationRepository.GetListAsync(cancellationToken: cancellationToken);
        var genderIdentities = await genderIdentityRepository.GetListAsync(cancellationToken: cancellationToken);

        return new StaffEqualityDetailsResponse
        {
            EthnicityId = person.EthnicityId,
            NationalityId = person.NationalityId,
            FirstLanguageId = person.FirstLanguageId,
            MaritalStatusId = person.MaritalStatusId,
            ReligionId = person.ReligionId,
            SexualOrientationId = person.SexualOrientationId,
            GenderIdentityId = person.GenderIdentityId,
            Ethnicities = ethnicities.ToOrderedLookup(),
            Nationalities = nationalities.ToAlphabeticalLookup(),
            Languages = languages.ToAlphabeticalLookup(),
            MaritalStatuses = maritalStatuses.ToAlphabeticalLookup(),
            Religions = religions.ToOrderedLookup(),
            SexualOrientations = sexualOrientations.ToOrderedLookup(),
            GenderIdentities = genderIdentities.ToOrderedLookup()
        };
    }

    public async Task UpdateEqualityDetailsAsync(Guid personId, StaffEqualityDetailsUpsertRequest model,
        CancellationToken cancellationToken, IUnitOfWork? uow = null)
    {
        var person = await personRepository.GetByIdAsync(personId, cancellationToken);

        if (person == null)
        {
            throw new NotFoundException("Person not found.");
        }

        person.EthnicityId = model.EthnicityId;
        person.NationalityId = model.NationalityId;
        person.FirstLanguageId = model.FirstLanguageId;
        person.MaritalStatusId = model.MaritalStatusId;
        person.ReligionId = model.ReligionId;
        person.SexualOrientationId = model.SexualOrientationId;
        person.GenderIdentityId = model.GenderIdentityId;

        await unitOfWorkFactory.RunInTransactionAsync(uow, async ownedUow =>
        {
            await personRepository.UpdateAsync(person, cancellationToken, ownedUow.Transaction);
        }, cancellationToken);
    }
}
