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
public class PersonEqualityService : IPersonEqualityService
{
    private readonly IPersonRepository _personRepository;
    private readonly IEthnicityRepository _ethnicityRepository;
    private readonly INationalityRepository _nationalityRepository;
    private readonly ILanguageRepository _languageRepository;
    private readonly IMaritalStatusRepository _maritalStatusRepository;
    private readonly IReligionRepository _religionRepository;
    private readonly ISexualOrientationRepository _sexualOrientationRepository;
    private readonly IGenderIdentityRepository _genderIdentityRepository;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public PersonEqualityService(IPersonRepository personRepository, IEthnicityRepository ethnicityRepository,
        INationalityRepository nationalityRepository, ILanguageRepository languageRepository,
        IMaritalStatusRepository maritalStatusRepository, IReligionRepository religionRepository,
        ISexualOrientationRepository sexualOrientationRepository, IGenderIdentityRepository genderIdentityRepository,
        IUnitOfWorkFactory unitOfWorkFactory)
    {
        _personRepository = personRepository;
        _ethnicityRepository = ethnicityRepository;
        _nationalityRepository = nationalityRepository;
        _languageRepository = languageRepository;
        _maritalStatusRepository = maritalStatusRepository;
        _religionRepository = religionRepository;
        _sexualOrientationRepository = sexualOrientationRepository;
        _genderIdentityRepository = genderIdentityRepository;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<StaffEqualityDetailsResponse> GetEqualityDetailsAsync(Guid personId,
        CancellationToken cancellationToken)
    {
        var person = await _personRepository.GetByIdAsync(personId, cancellationToken);

        if (person == null)
        {
            throw new NotFoundException("Person not found.");
        }

        var ethnicities = await _ethnicityRepository.GetListAsync(cancellationToken: cancellationToken);
        var nationalities = await _nationalityRepository.GetListAsync(cancellationToken: cancellationToken);
        var languages = await _languageRepository.GetListAsync(cancellationToken: cancellationToken);
        var maritalStatuses = await _maritalStatusRepository.GetListAsync(cancellationToken: cancellationToken);
        var religions = await _religionRepository.GetListAsync(cancellationToken: cancellationToken);
        var sexualOrientations = await _sexualOrientationRepository.GetListAsync(cancellationToken: cancellationToken);
        var genderIdentities = await _genderIdentityRepository.GetListAsync(cancellationToken: cancellationToken);

        return new StaffEqualityDetailsResponse
        {
            EthnicityId = person.EthnicityId,
            NationalityId = person.NationalityId,
            FirstLanguageId = person.FirstLanguageId,
            MaritalStatusId = person.MaritalStatusId,
            ReligionId = person.ReligionId,
            SexualOrientationId = person.SexualOrientationId,
            GenderIdentityId = person.GenderIdentityId,
            Ethnicities = ethnicities.ToOrderedLookups(),
            Nationalities = nationalities.ToAlphabeticalLookups(),
            Languages = languages.ToAlphabeticalLookups(),
            MaritalStatuses = maritalStatuses.ToAlphabeticalLookups(),
            Religions = religions.ToOrderedLookups(),
            SexualOrientations = sexualOrientations.ToOrderedLookups(),
            GenderIdentities = genderIdentities.ToOrderedLookups()
        };
    }

    public async Task UpdateEqualityDetailsAsync(Guid personId, StaffEqualityDetailsUpsertRequest model,
        CancellationToken cancellationToken, IUnitOfWork? uow = null)
    {
        var person = await _personRepository.GetByIdAsync(personId, cancellationToken);

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

        await _unitOfWorkFactory.RunInTransactionAsync(uow, async ownedUow =>
        {
            await _personRepository.UpdateAsync(person, cancellationToken, ownedUow.Transaction);
        }, cancellationToken);
    }
}
