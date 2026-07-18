using System.Data;
using FluentValidation;
using FluentValidation.Results;
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
/// Shared person-level address mechanics. Keyed by personId and deliberately auth-free — the
/// subtype service that calls in (staff/student/contact) owns access control. See
/// <see cref="IPersonAddressService"/>.
/// </summary>
public class PersonAddressService(
    IAddressRepository addressRepository,
    IAddressPersonRepository addressPersonRepository,
    IAddressTypeRepository addressTypeRepository,
    IValidationService validationService,
    IUnitOfWorkFactory unitOfWorkFactory)
    : IPersonAddressService
{
    private const int MinSearchLength = 2;

    public async Task<AddressListResponse> GetAddressesAsync(Guid personId, CancellationToken cancellationToken)
    {
        var addresses = await addressRepository.GetByPersonIdAsync(personId, cancellationToken);
        var types = await addressTypeRepository.GetListAsync(cancellationToken: cancellationToken);

        return new AddressListResponse
        {
            Addresses = addresses.ToList(),
            AddressTypes = types.Where(t => t.Active).Select(t => t.ToResponseModel()).ToList()
        };
    }

    public async Task<IReadOnlyList<AddressMatchResponse>> SearchAddressesAsync(string? query,
        CancellationToken cancellationToken)
    {
        var trimmed = query?.Trim();

        if (string.IsNullOrEmpty(trimmed) || trimmed.Length < MinSearchLength)
        {
            return [];
        }

        var escaped = trimmed.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]");

        return await addressRepository.SearchAsync($"%{escaped}%", cancellationToken);
    }

    public async Task<Guid> AddAddressAsync(Guid personId, PersonAddressUpsertRequest model,
        CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(model);

        return await unitOfWorkFactory.RunInTransactionAsync<Guid>(null, async uow =>
        {
            var tx = uow.Transaction;

            Guid addressId;

            if (model.ExistingAddressId.HasValue)
            {
                var existing = await addressRepository.GetByIdAsync(model.ExistingAddressId.Value,
                    cancellationToken, tx);

                if (existing == null)
                {
                    throw new NotFoundException("Address not found.");
                }

                addressId = existing.Id;

                // Don't double-link the same address to the same person.
                var links = await addressPersonRepository.GetByPersonIdAsync(personId, cancellationToken, tx);
                if (links.Any(l => l.AddressId == addressId))
                {
                    throw new ValidationException([
                        new ValidationFailure(nameof(model.ExistingAddressId),
                            "This person is already linked to that address.")
                    ]);
                }
            }
            else
            {
                // Dedupe: reuse an existing address with the same key rather than inserting a twin.
                var match = await addressRepository.FindByMatchKeyAsync(model.Postcode!, model.BuildingNumber,
                    model.BuildingName, model.Street!, cancellationToken, tx);

                if (match != null)
                {
                    addressId = match.Id;
                }
                else
                {
                    var address = new Address
                    {
                        Id = SqlConvention.SequentialGuid(),
                        BuildingNumber = model.BuildingNumber,
                        BuildingName = model.BuildingName,
                        Apartment = model.Apartment,
                        Street = model.Street!,
                        District = model.District,
                        Town = model.Town!,
                        County = model.County!,
                        Postcode = model.Postcode!,
                        Country = model.Country!,
                        IsValidated = false
                    };
                    await addressRepository.InsertAsync(address, cancellationToken, tx);
                    addressId = address.Id;
                }
            }

            if (model.IsMain)
            {
                await ClearMainAsync(personId, null, tx, cancellationToken);
            }

            var link = new AddressPerson
            {
                Id = SqlConvention.SequentialGuid(),
                AddressId = addressId,
                PersonId = personId,
                AddressTypeId = model.TypeId,
                IsMain = model.IsMain
            };
            await addressPersonRepository.InsertAsync(link, cancellationToken, tx);

            return link.Id;
        }, cancellationToken);
    }

    public async Task UpdateAddressAsync(Guid personId, Guid addressPersonId,
        PersonAddressUpdateRequest model, CancellationToken cancellationToken)
    {
        await validationService.ValidateAsync(model);

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var tx = uow.Transaction;

            var link = await addressPersonRepository.GetByIdAsync(addressPersonId, cancellationToken, tx);

            // Not found, or not this person's link — same 404 either way so we don't leak existence.
            if (link == null || link.PersonId != personId)
            {
                throw new NotFoundException("Address not found.");
            }

            var address = await addressRepository.GetByIdAsync(link.AddressId, cancellationToken, tx);
            if (address == null)
            {
                throw new NotFoundException("Address not found.");
            }

            // Is the address shared with anyone else? (SharedCount includes this person.)
            var personAddresses = await addressRepository.GetByPersonIdAsync(personId, cancellationToken, tx);
            var sharedByOthers =
                personAddresses.FirstOrDefault(a => a.AddressPersonId == addressPersonId)?.SharedCount > 1;

            if (sharedByOthers && model.Mode == AddressEditMode.Moved)
            {
                // They moved — fork a fresh address with the edits and relink just this person.
                var forked = new Address
                {
                    Id = SqlConvention.SequentialGuid(),
                    BuildingNumber = model.BuildingNumber,
                    BuildingName = model.BuildingName,
                    Apartment = model.Apartment,
                    Street = model.Street,
                    District = model.District,
                    Town = model.Town,
                    County = model.County,
                    Postcode = model.Postcode,
                    Country = model.Country,
                    IsValidated = false
                };
                await addressRepository.InsertAsync(forked, cancellationToken, tx);
                link.AddressId = forked.Id;
            }
            else
            {
                // Sole occupant, or an explicit fix-for-everyone — edit the shared row in place.
                address.BuildingNumber = model.BuildingNumber;
                address.BuildingName = model.BuildingName;
                address.Apartment = model.Apartment;
                address.Street = model.Street;
                address.District = model.District;
                address.Town = model.Town;
                address.County = model.County;
                address.Postcode = model.Postcode;
                address.Country = model.Country;
                await addressRepository.UpdateAsync(address, cancellationToken, tx);
            }

            if (model.IsMain)
            {
                await ClearMainAsync(personId, addressPersonId, tx, cancellationToken);
            }

            link.AddressTypeId = model.TypeId;
            link.IsMain = model.IsMain;
            await addressPersonRepository.UpdateAsync(link, cancellationToken, tx);
        }, cancellationToken);
    }

    public async Task RemoveAddressAsync(Guid personId, Guid addressPersonId,
        CancellationToken cancellationToken)
    {
        var link = await addressPersonRepository.GetByIdAsync(addressPersonId, cancellationToken);

        if (link == null || link.PersonId != personId)
        {
            throw new NotFoundException("Address not found.");
        }

        // Soft-delete the link only — the shared address survives for anyone else linked to it.
        await addressPersonRepository.DeleteAsync(addressPersonId, cancellationToken);
    }

    // Enforce one-main-per-person: clear IsMain on the person's other links before setting a new one.
    private async Task ClearMainAsync(Guid personId, Guid? exceptLinkId, IDbTransaction? tx,
        CancellationToken cancellationToken)
    {
        var links = await addressPersonRepository.GetByPersonIdAsync(personId, cancellationToken, tx);

        foreach (var link in links)
        {
            if (link.Id != exceptLinkId && link.IsMain)
            {
                link.IsMain = false;
                await addressPersonRepository.UpdateAsync(link, cancellationToken, tx);
            }
        }
    }
}
