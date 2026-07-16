using System.Data;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;

namespace MyPortal.Data.Interfaces;

public interface IAddressRepository : IEntityRepository<Address>
{
    /// <summary>A person's linked addresses joined to the shared rows, with a shared-people count.</summary>
    Task<IReadOnlyList<PersonAddressResponse>> GetByPersonIdAsync(Guid personId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    /// <summary>
    /// Search existing addresses for the search-before-add flow. <paramref name="like"/> is the
    /// caller-built contains pattern. Capped at 25.
    /// </summary>
    Task<IReadOnlyList<AddressMatchResponse>> SearchAsync(string like, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    /// <summary>
    /// Find an existing address by its normalised match key (postcode + building + street),
    /// case/whitespace-insensitive. Null if none. Used to dedupe before inserting a new address.
    /// </summary>
    Task<Address?> FindByMatchKeyAsync(string postcode, string? buildingNumber, string? buildingName,
        string street, CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
