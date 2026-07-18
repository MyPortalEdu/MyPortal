using System.Data;
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
/// Shared person-level email/phone mechanics. Keyed by personId and deliberately auth-free — the
/// subtype service that calls in (staff/student/contact) owns access control. See
/// <see cref="IPersonContactService"/>.
/// </summary>
public class PersonContactService(
    IEmailAddressRepository emailRepository,
    IPhoneNumberRepository phoneRepository,
    IEmailAddressTypeRepository emailTypeRepository,
    IPhoneNumberTypeRepository phoneTypeRepository,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : IPersonContactService
{
    public async Task<StaffContactDetailsResponse> GetContactDetailsAsync(Guid personId,
        CancellationToken cancellationToken)
    {
        var emails = await emailRepository.GetByPersonIdAsync(personId, cancellationToken);
        var phones = await phoneRepository.GetByPersonIdAsync(personId, cancellationToken);
        var emailTypes = await emailTypeRepository.GetListAsync(cancellationToken: cancellationToken);
        var phoneTypes = await phoneTypeRepository.GetListAsync(cancellationToken: cancellationToken);

        return new StaffContactDetailsResponse
        {
            Emails = emails.Select(e => new PersonEmailResponse
            {
                Id = e.Id,
                TypeId = e.TypeId,
                Address = e.Address,
                IsMain = e.IsMain,
                Notes = e.Notes
            }).ToList(),
            Phones = phones.Select(p => new PersonPhoneResponse
            {
                Id = p.Id,
                TypeId = p.TypeId,
                Number = p.Number,
                IsMain = p.IsMain
            }).ToList(),
            EmailTypes = emailTypes.ToAlphabeticalLookup(),
            PhoneTypes = phoneTypes.ToOrderedLookup()
        };
    }

    public async Task UpdateContactDetailsAsync(Guid personId, StaffContactDetailsUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(model);

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await ReconcileEmailsAsync(personId, model.Emails, uow.Transaction, cancellationToken);
            await ReconcilePhonesAsync(personId, model.Phones, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    private async Task ReconcileEmailsAsync(Guid personId, List<PersonEmailUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing = await emailRepository.GetByPersonIdAsync(personId, cancellationToken, transaction);
        var existingById = existing.ToDictionary(e => e.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        // Soft-delete rows the payload dropped.
        foreach (var row in existing)
        {
            if (!keptIds.Contains(row.Id))
            {
                await emailRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
            }
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
                // Null id, or an id we don't own — treat as a new row rather than trusting it.
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

    private async Task ReconcilePhonesAsync(Guid personId, List<PersonPhoneUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing = await phoneRepository.GetByPersonIdAsync(personId, cancellationToken, transaction);
        var existingById = existing.ToDictionary(p => p.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        foreach (var row in existing)
        {
            if (!keptIds.Contains(row.Id))
            {
                await phoneRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
            }
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
}
