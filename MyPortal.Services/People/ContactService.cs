using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.Documents;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

public class ContactService(
    IAuthorizationService authorizationService,
    ILogger<ContactService> logger,
    IContactRepository contactRepository,
    IPersonRepository personRepository,
    IPersonService personService,
    IPersonContactService personContactService,
    IPersonAddressService personAddressService,
    IStudentContactRelationshipRepository relationshipRepository,
    IPhotoService photoService,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IContactService
{
    public async Task<IReadOnlyList<ContactStudentResponse>> GetAssociatedStudentsAsync(Guid id,
        CancellationToken cancellationToken)
    {
        var contact = await contactRepository.GetByIdAsync(id, cancellationToken);

        if (contact == null)
        {
            throw new NotFoundException("Contact not found.");
        }

        return await relationshipRepository.GetByContactIdAsync(id, cancellationToken);
    }

    /// <summary>Minimum query length before <see cref="SearchPeopleAsync"/> hits the database.</summary>
    private const int MinSearchLength = 2;

    public async Task<PageResult<ContactSummaryResponse>> GetContactsAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default)
    {
        return await contactRepository.GetContactsAsync(filter, sort, paging, cancellationToken);
    }

    public async Task<ContactHeaderResponse> GetHeaderAsync(Guid id, CancellationToken cancellationToken)
    {
        var row = await contactRepository.GetHeaderByIdAsync(id, cancellationToken);

        if (row == null)
        {
            throw new NotFoundException("Contact not found.");
        }

        return new ContactHeaderResponse
        {
            Id = row.Id,
            PersonId = row.PersonId,
            DisplayName = row.DisplayName,
            PreferredName = row.PreferredName,
            PhotoId = row.PhotoId
        };
    }

    public async Task<ContactBasicDetailsResponse> GetBasicDetailsAsync(Guid id, CancellationToken cancellationToken)
    {
        var details = await contactRepository.GetBasicDetailsByIdAsync(id, cancellationToken);

        if (details == null)
        {
            throw new NotFoundException("Contact not found.");
        }

        return details;
    }

    public async Task UpdateBasicDetailsAsync(Guid id, ContactBasicDetailsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(model);

        var contact = await contactRepository.GetByIdAsync(id, cancellationToken);

        if (contact == null)
        {
            throw new NotFoundException("Contact not found.");
        }

        // This area writes both the shared Person bio AND the Contact-row fields, in one transaction.
        await unitOfWorkFactory.RunInTransactionAsync(null, async ownedUow =>
        {
            await personService.UpdateBasicBioAsync(contact.PersonId, ToBio(model), cancellationToken, ownedUow);

            contact.ParentalBallot = model.ParentalBallot;
            contact.PlaceOfWork = Normalise(model.PlaceOfWork);
            contact.JobTitle = Normalise(model.JobTitle);
            contact.NiNumber = Normalise(model.NiNumber);

            await contactRepository.UpdateAsync(contact, cancellationToken, ownedUow.Transaction);
        }, cancellationToken);
    }

    public async Task<PersonContactDetailsResponse> GetContactDetailsAsync(Guid id,
        CancellationToken cancellationToken)
    {
        var personId = await ResolvePersonIdAsync(id, cancellationToken);

        return await personContactService.GetContactDetailsAsync(personId, cancellationToken);
    }

    public async Task UpdateContactDetailsAsync(Guid id, PersonContactDetailsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        var personId = await ResolvePersonIdAsync(id, cancellationToken);

        await personContactService.UpdateContactDetailsAsync(personId, model, cancellationToken);
    }

    public async Task<AddressListResponse> GetAddressesAsync(Guid id, CancellationToken cancellationToken)
    {
        var personId = await ResolvePersonIdAsync(id, cancellationToken);

        return await personAddressService.GetAddressesAsync(personId, cancellationToken);
    }

    public async Task<IReadOnlyList<AddressMatchResponse>> SearchAddressesAsync(string? query,
        CancellationToken cancellationToken)
    {
        return await personAddressService.SearchAddressesAsync(query, cancellationToken);
    }

    public async Task<Guid> AddAddressAsync(Guid id, PersonAddressUpsertRequest model,
        CancellationToken cancellationToken)
    {
        var personId = await ResolvePersonIdAsync(id, cancellationToken);

        return await personAddressService.AddAddressAsync(personId, model, cancellationToken);
    }

    public async Task UpdateAddressAsync(Guid id, Guid addressPersonId, PersonAddressUpdateRequest model,
        CancellationToken cancellationToken)
    {
        var personId = await ResolvePersonIdAsync(id, cancellationToken);

        await personAddressService.UpdateAddressAsync(personId, addressPersonId, model, cancellationToken);
    }

    public async Task RemoveAddressAsync(Guid id, Guid addressPersonId, CancellationToken cancellationToken)
    {
        var personId = await ResolvePersonIdAsync(id, cancellationToken);

        await personAddressService.RemoveAddressAsync(personId, addressPersonId, cancellationToken);
    }

    private async Task<Guid> ResolvePersonIdAsync(Guid contactId, CancellationToken cancellationToken)
    {
        var contact = await contactRepository.GetByIdAsync(contactId, cancellationToken);

        if (contact == null)
        {
            throw new NotFoundException("Contact not found.");
        }

        return contact.PersonId;
    }

    public async Task SetPhotoAsync(Guid id, Stream image, string contentType, string fileName,
        CancellationToken cancellationToken)
    {
        var contact = await contactRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("Contact not found.");

        await photoService.SetPhotoAsync(contact.PersonId, image, contentType, fileName, cancellationToken);
    }

    public async Task<DocumentContentResponse> GetPhotoAsync(Guid id, CancellationToken cancellationToken)
    {
        var contact = await contactRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("Contact not found.");

        return await photoService.GetPhotoContentAsync(contact.PersonId, cancellationToken);
    }

    public async Task DeletePhotoAsync(Guid id, CancellationToken cancellationToken)
    {
        var contact = await contactRepository.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("Contact not found.");

        await photoService.DeletePhotoAsync(contact.PersonId, cancellationToken);
    }

    public async Task<Guid> CreateAsync(ContactBasicDetailsUpsertRequest model, CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(model);

        var contactId = SqlConvention.SequentialGuid();

        return await unitOfWorkFactory.RunInTransactionAsync<Guid>(null, async ownedUow =>
        {
            // Person row + its directory first; the contact row hangs off the new person, both in
            // one transaction.
            var personId = await personService.CreateAsync(ToBio(model), cancellationToken, ownedUow);

            var contact = new Contact
            {
                Id = contactId,
                PersonId = personId,
                ParentalBallot = model.ParentalBallot,
                PlaceOfWork = Normalise(model.PlaceOfWork),
                JobTitle = Normalise(model.JobTitle),
                NiNumber = Normalise(model.NiNumber)
            };

            await contactRepository.InsertAsync(contact, cancellationToken, ownedUow.Transaction);

            return contactId;
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<ContactMatchResponse>> SearchPeopleAsync(string? query,
        CancellationToken cancellationToken)
    {
        var trimmed = query?.Trim();

        if (string.IsNullOrEmpty(trimmed) || trimmed.Length < MinSearchLength)
        {
            return [];
        }

        // Escape LIKE wildcards in the user term so '%'/'_' are matched literally, then wrap as a
        // contains pattern. '[' must be escaped first.
        var escaped = trimmed.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

        return await contactRepository.SearchPeopleForContactCreateAsync($"%{escaped}%", cancellationToken);
    }

    public async Task<Guid> CreateForPersonAsync(ContactCreateForPersonRequest model,
        CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(model);

        var person = await personRepository.GetByIdAsync(model.PersonId, cancellationToken);

        if (person == null)
        {
            throw new NotFoundException("Person not found.");
        }

        // A person can only hold one contact role — block a duplicate Contact row. The FE already
        // disables already-contact matches; this is the server-side guard.
        var existing = await contactRepository.GetContactIdByPersonIdAsync(model.PersonId, cancellationToken);

        if (existing != null)
        {
            throw new ValidationException(
                [new ValidationFailure(nameof(model.PersonId), "This person is already a contact.")]);
        }

        var contactId = SqlConvention.SequentialGuid();

        return await unitOfWorkFactory.RunInTransactionAsync<Guid>(null, async ownedUow =>
        {
            // Only the Contact row is created — the Person (and its bio + directory) already exists.
            var contact = new Contact
            {
                Id = contactId,
                PersonId = model.PersonId
            };

            await contactRepository.InsertAsync(contact, cancellationToken, ownedUow.Transaction);

            return contactId;
        }, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var contact = await contactRepository.GetByIdAsync(id, cancellationToken);

        if (contact == null)
        {
            throw new NotFoundException("Contact not found.");
        }

        // Soft-delete the contact row only — the person may also be a student/staff member.
        await contactRepository.DeleteAsync(id, cancellationToken);
    }

    private static string? Normalise(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static PersonBasicBio ToBio(ContactBasicDetailsUpsertRequest model) => new(
        Title: model.Title,
        FirstName: model.FirstName,
        MiddleName: model.MiddleName,
        LastName: model.LastName,
        PreferredFirstName: model.PreferredFirstName,
        PreferredLastName: model.PreferredLastName,
        PhotoId: model.PhotoId,
        Gender: model.Gender,
        Dob: model.Dob,
        Deceased: model.Deceased);
}
