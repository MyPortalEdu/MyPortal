using System.Data;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

/// <summary>
/// Owns the Emergency Contacts area. Each next-of-kin is a link (NextOfKin) from the staff member
/// to a shared Contact record — a Person facet — so a contact can be reused across relationships
/// and carries their own phones/emails. Gating is <see cref="StaffArea.EmergencyContacts"/>,
/// All-scope view/edit only. The save whole-area replaces the links; adding a contact either
/// reuses an existing person (search-before-add) or creates a fresh one.
/// </summary>
public class StaffNextOfKinService(
    IAuthorizationService authorizationService,
    ILogger<StaffNextOfKinService> logger,
    IStaffMemberAccessService accessService,
    IStaffMemberRepository staffMemberRepository,
    INextOfKinRepository nextOfKinRepository,
    IContactRepository contactRepository,
    IPersonRepository personRepository,
    IPersonService personService,
    INextOfKinRelationshipTypeRepository relationshipTypeRepository,
    IEmailAddressRepository emailRepository,
    IPhoneNumberRepository phoneRepository,
    IEmailAddressTypeRepository emailTypeRepository,
    IPhoneNumberTypeRepository phoneTypeRepository,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : BaseService(authorizationService, logger), IStaffNextOfKinService
{
    public async Task<StaffNextOfKinAreaResponse> GetNextOfKinAsync(Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(staffMemberId, StaffArea.EmergencyContacts, StaffAccess.ViewAll,
            cancellationToken);

        var staffMember = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        var links = (await nextOfKinRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken))
            .OrderBy(l => l.ContactOrder)
            .ToList();

        var contacts = new List<StaffNextOfKinResponse>();

        foreach (var link in links)
        {
            var contact = await contactRepository.GetByIdAsync(link.ContactId, cancellationToken);
            var person = contact == null ? null : await personRepository.GetByIdAsync(contact.PersonId, cancellationToken);

            if (contact == null || person == null)
            {
                continue;
            }

            var emails = await emailRepository.GetByPersonIdAsync(person.Id, cancellationToken);
            var phones = await phoneRepository.GetByPersonIdAsync(person.Id, cancellationToken);

            contacts.Add(new StaffNextOfKinResponse
            {
                Id = link.Id,
                ContactId = contact.Id,
                PersonId = person.Id,
                Title = person.Title,
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                LastName = person.LastName,
                RelationshipTypeId = link.RelationshipTypeId,
                ContactOrder = link.ContactOrder,
                Notes = link.Notes,
                Emails = emails.Select(MapEmail).ToList(),
                Phones = phones.Select(MapPhone).ToList()
            });
        }

        var relationshipTypes = await relationshipTypeRepository.GetListAsync(cancellationToken: cancellationToken);
        var phoneTypes = await phoneTypeRepository.GetListAsync(cancellationToken: cancellationToken);
        var emailTypes = await emailTypeRepository.GetListAsync(cancellationToken: cancellationToken);

        return new StaffNextOfKinAreaResponse
        {
            Contacts = contacts,
            RelationshipTypes = relationshipTypes.ToOrderedLookup(),
            PhoneTypes = phoneTypes.ToOrderedLookup(),
            EmailTypes = emailTypes.ToAlphabeticalLookup()
        };
    }

    public async Task UpdateNextOfKinAsync(Guid staffMemberId, StaffNextOfKinAreaUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await accessService.RequireAsync(staffMemberId, StaffArea.EmergencyContacts, StaffAccess.EditAll,
            cancellationToken);

        await validationService.ValidateAsync(model);

        var staffMember = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await ReconcileAsync(staffMemberId, model.Contacts, uow, cancellationToken);
        }, cancellationToken);
    }

    private async Task ReconcileAsync(Guid staffMemberId, List<StaffNextOfKinUpsertItem> incoming,
        IUnitOfWork uow, CancellationToken cancellationToken)
    {
        var transaction = uow.Transaction;

        var existing = (await nextOfKinRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction))
            .ToList();
        var existingById = existing.ToDictionary(x => x.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        // Unlink dropped rows — soft-delete the link only; the shared Contact is left intact.
        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await nextOfKinRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
        }

        foreach (var item in incoming)
        {
            Guid personId;

            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var link))
            {
                var contact = await contactRepository.GetByIdAsync(link.ContactId, cancellationToken);

                if (contact == null)
                {
                    continue;
                }

                personId = contact.PersonId;
                await personService.UpdateBasicBioAsync(personId, ToBio(item), cancellationToken, uow);

                ApplyLink(link, item, link.ContactId);
                await nextOfKinRepository.UpdateAsync(link, cancellationToken, transaction);
            }
            else
            {
                var (contactId, newPersonId) = await ResolveContactAsync(item, uow, cancellationToken);
                personId = newPersonId;

                var created = new NextOfKin
                {
                    Id = SqlConvention.SequentialGuid(),
                    StaffMemberId = staffMemberId
                };
                ApplyLink(created, item, contactId);
                await nextOfKinRepository.InsertAsync(created, cancellationToken, transaction);
            }

            await ReconcilePhonesAsync(personId, item.Phones, transaction, cancellationToken);
            await ReconcileEmailsAsync(personId, item.Emails, transaction, cancellationToken);
        }
    }

    // Resolve the Contact for a new link: reuse an existing person (search-before-add) or create a
    // fresh one, then ensure that person has a Contact facet.
    private async Task<(Guid contactId, Guid personId)> ResolveContactAsync(StaffNextOfKinUpsertItem item,
        IUnitOfWork uow, CancellationToken cancellationToken)
    {
        var personId = item.PersonId ?? await personService.CreateAsync(ToBio(item), cancellationToken, uow);

        var existingContact = await contactRepository.GetByPersonIdAsync(personId, cancellationToken, uow.Transaction);

        if (existingContact != null)
        {
            return (existingContact.Id, personId);
        }

        var contact = new Contact { Id = SqlConvention.SequentialGuid(), PersonId = personId };
        await contactRepository.InsertAsync(contact, cancellationToken, uow.Transaction);
        return (contact.Id, personId);
    }

    private static void ApplyLink(NextOfKin link, StaffNextOfKinUpsertItem item, Guid contactId)
    {
        link.ContactId = contactId;
        link.RelationshipTypeId = item.RelationshipTypeId;
        link.ContactOrder = item.ContactOrder;
        link.Notes = item.Notes;
    }

    private static PersonBasicBio ToBio(StaffNextOfKinUpsertItem item) => new(
        Title: item.Title,
        FirstName: item.FirstName,
        MiddleName: item.MiddleName,
        LastName: item.LastName,
        PreferredFirstName: null,
        PreferredLastName: null,
        PhotoId: null,
        // Gender is unknown for most next-of-kin; default to "U" (the People column is required).
        Gender: string.IsNullOrWhiteSpace(item.Gender) ? "U" : item.Gender!,
        Dob: null,
        Deceased: null);

    private async Task ReconcilePhonesAsync(Guid personId, List<PersonPhoneUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing = await phoneRepository.GetByPersonIdAsync(personId, cancellationToken, transaction);
        var existingById = existing.ToDictionary(p => p.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await phoneRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                entity.TypeId = item.TypeId;
                entity.Number = item.Number;
                entity.IsMain = item.IsMain;
                await phoneRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                await phoneRepository.InsertAsync(new PhoneNumber
                {
                    Id = SqlConvention.SequentialGuid(),
                    PersonId = personId,
                    TypeId = item.TypeId,
                    Number = item.Number,
                    IsMain = item.IsMain
                }, cancellationToken, transaction);
            }
        }
    }

    private async Task ReconcileEmailsAsync(Guid personId, List<PersonEmailUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing = await emailRepository.GetByPersonIdAsync(personId, cancellationToken, transaction);
        var existingById = existing.ToDictionary(e => e.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing.Where(row => !keptIds.Contains(row.Id)))
        {
            await emailRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                entity.TypeId = item.TypeId;
                entity.Address = item.Address;
                entity.IsMain = item.IsMain;
                entity.Notes = item.Notes;
                await emailRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                await emailRepository.InsertAsync(new EmailAddress
                {
                    Id = SqlConvention.SequentialGuid(),
                    PersonId = personId,
                    TypeId = item.TypeId,
                    Address = item.Address,
                    IsMain = item.IsMain,
                    Notes = item.Notes
                }, cancellationToken, transaction);
            }
        }
    }

    private static PersonEmailResponse MapEmail(EmailAddress e) => new()
    {
        Id = e.Id,
        TypeId = e.TypeId,
        Address = e.Address,
        IsMain = e.IsMain,
        Notes = e.Notes
    };

    private static PersonPhoneResponse MapPhone(PhoneNumber p) => new()
    {
        Id = p.Id,
        TypeId = p.TypeId,
        Number = p.Number,
        IsMain = p.IsMain
    };
}
