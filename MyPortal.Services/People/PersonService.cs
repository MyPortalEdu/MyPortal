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
using MyPortal.Services.Interfaces;
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
/// </summary>
public class PersonService : BaseService, IPersonService
{
    private readonly IPersonRepository _personRepository;
    private readonly IDirectoryService _directoryService;
    private readonly IValidationService _validationService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public PersonService(IAuthorizationService authorizationService, ILogger<PersonService> logger,
        IPersonRepository personRepository, IDirectoryService directoryService, IValidationService validationService,
        IUnitOfWorkFactory unitOfWorkFactory) : base(authorizationService, logger)
    {
        _personRepository = personRepository;
        _directoryService = directoryService;
        _validationService = validationService;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<Guid> CreateAsync(PersonUpsertRequest model, CancellationToken cancellationToken,
        IUnitOfWork? uow = null)
    {
        await _validationService.ValidateAsync(model);

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
                Title = model.Title,
                PreferredFirstName = model.PreferredFirstName,
                PreferredLastName = model.PreferredLastName,
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                PhotoId = model.PhotoId,
                NhsNumber = model.NhsNumber,
                Gender = model.Gender,
                Dob = model.Dob,
                Deceased = model.Deceased,
                EthnicityId = model.EthnicityId,
                NationalityId = model.NationalityId,
                FirstLanguageId = model.FirstLanguageId,
                MaritalStatusId = model.MaritalStatusId,
                IsDeleted = false
            };

            await _personRepository.InsertAsync(person, cancellationToken, ownedUow.Transaction);

            return personId;
        }, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, PersonUpsertRequest model, CancellationToken cancellationToken,
        IUnitOfWork? uow = null)
    {
        await _validationService.ValidateAsync(model);

        var person = await _personRepository.GetByIdAsync(id, cancellationToken);

        if (person == null)
        {
            throw new NotFoundException("Person not found.");
        }

        person.Title = model.Title;
        person.PreferredFirstName = model.PreferredFirstName;
        person.PreferredLastName = model.PreferredLastName;
        person.FirstName = model.FirstName;
        person.MiddleName = model.MiddleName;
        person.LastName = model.LastName;
        person.PhotoId = model.PhotoId;
        person.NhsNumber = model.NhsNumber;
        person.Gender = model.Gender;
        person.Dob = model.Dob;
        person.Deceased = model.Deceased;
        person.EthnicityId = model.EthnicityId;
        person.NationalityId = model.NationalityId;
        person.FirstLanguageId = model.FirstLanguageId;
        person.MaritalStatusId = model.MaritalStatusId;

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
        }, cancellationToken);
    }
}
