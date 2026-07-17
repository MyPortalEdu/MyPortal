using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces.Documents;
using MyPortal.Services.Interfaces.People;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

/// <summary>
/// Infrastructure for the shared <c>Person</c> record. A person only ever exists as a
/// subtype (staff member, student, contact, agent), so this service is composed by the
/// subtype services and performs NO authorization of its own — the capability a user is
/// granted is "edit staff basic details", "edit student details" etc., never "edit a
/// person" in the abstract. Each method may take an <see cref="IUnitOfWork"/> to join the
/// caller's transaction. Do not expose these directly from a controller without the calling
/// layer applying the appropriate subtype permission gate.
///
/// Both Create and UpdateBasicBio take the shared <see cref="PersonBasicBio"/> record, so
/// equality-sensitive fields (NhsNumber, EthnicityId) are never set through here — they're
/// populated post-creation via the equality-area endpoint when that ships.
/// </summary>
public class PersonService : BaseService, IPersonService
{
    private readonly IPersonRepository _personRepository;
    private readonly IDirectoryService _directoryService;
    private readonly IPhotoService _photoService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public PersonService(IAuthorizationService authorizationService, ILogger<PersonService> logger,
        IPersonRepository personRepository, IDirectoryService directoryService, IPhotoService photoService,
        IUnitOfWorkFactory unitOfWorkFactory) : base(authorizationService, logger)
    {
        _personRepository = personRepository;
        _directoryService = directoryService;
        _photoService = photoService;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<Guid> CreateAsync(PersonBasicBio bio, CancellationToken cancellationToken,
        IUnitOfWork? uow = null)
    {
        var personId = SqlConvention.SequentialGuid();

        // Each person owns a private directory subtree for their documents.
        var directoryRequest = new DirectoryUpsertRequest
        {
            Name = $"person-{personId:N}",
            IsPrivate = true,
            UploadPolicy = DirectoryUploadPolicy.StaffOnly
        };

        return await _unitOfWorkFactory.RunInTransactionAsync<Guid>(uow, async ownedUow =>
        {
            var directory = await _directoryService.CreateAsync(directoryRequest, cancellationToken, ownedUow);

            var person = new Person
            {
                Id = personId,
                DirectoryId = directory.Id,
                Title = bio.Title,
                FirstName = bio.FirstName,
                MiddleName = bio.MiddleName,
                LastName = bio.LastName,
                PreferredFirstName = bio.PreferredFirstName,
                PreferredLastName = bio.PreferredLastName,
                PhotoId = bio.PhotoId,
                Gender = bio.Gender,
                Dob = bio.Dob,
                Deceased = bio.Deceased
                // Equality fields (NhsNumber, EthnicityId, NationalityId, FirstLanguageId,
                // MaritalStatusId, ReligionId, SexualOrientationId, GenderIdentityId) are
                // deliberately omitted — they're populated via the equality-area PUT.
            };

            await _personRepository.InsertAsync(person, cancellationToken, ownedUow.Transaction);

            return personId;
        }, cancellationToken);
    }

    public async Task UpdateBasicBioAsync(Guid personId, PersonBasicBio bio, CancellationToken cancellationToken,
        IUnitOfWork? uow = null)
    {
        var person = await _personRepository.GetByIdAsync(personId, cancellationToken);

        if (person == null)
        {
            throw new NotFoundException("Person not found.");
        }

        person.Title = bio.Title;
        person.FirstName = bio.FirstName;
        person.MiddleName = bio.MiddleName;
        person.LastName = bio.LastName;
        person.PreferredFirstName = bio.PreferredFirstName;
        person.PreferredLastName = bio.PreferredLastName;
        person.PhotoId = bio.PhotoId;
        person.Gender = bio.Gender;
        person.Dob = bio.Dob;
        person.Deceased = bio.Deceased;
        // Equality fields (NhsNumber, EthnicityId, NationalityId, FirstLanguageId,
        // MaritalStatusId, ReligionId, SexualOrientationId, GenderIdentityId) are not
        // touched here — they're owned by the equality-area update.

        await _unitOfWorkFactory.RunInTransactionAsync(uow, async ownedUow =>
        {
            await _personRepository.UpdateAsync(person, cancellationToken, ownedUow.Transaction);
        }, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken, IUnitOfWork? uow = null)
    {
        var person = await _personRepository.GetByIdAsync(id, cancellationToken);

        if (person == null)
        {
            throw new NotFoundException("Person not found.");
        }

        await _unitOfWorkFactory.RunInTransactionAsync(uow, async ownedUow =>
        {
            // Person → directory FK: drop the person row first to release it.
            await _personRepository.DeleteAsync(id, cancellationToken, softDelete: false, ownedUow.Transaction);

            await _directoryService.DeleteAsync(person.DirectoryId, cancellationToken, ownedUow, softDelete: false);

            // The photo document lives in the system Photos directory, outside this person's
            // directory subtree, so the cascade above doesn't reach it — purge it explicitly. Safe
            // now the person row (and its PhotoId FK) is gone.
            if (person.PhotoId is not null)
            {
                await _photoService.PurgePhotoAsync(person.PhotoId.Value, cancellationToken, ownedUow);
            }
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<PersonSearchResponse>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var term = query?.Trim() ?? string.Empty;
        if (term.Length < 2)
        {
            return Array.Empty<PersonSearchResponse>();
        }

        // Escape LIKE wildcards ([, %, _) before wrapping in a contains pattern. '[' must go first.
        var escaped = term.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");
        return await _personRepository.SearchAsync($"%{escaped}%", cancellationToken);
    }
}
