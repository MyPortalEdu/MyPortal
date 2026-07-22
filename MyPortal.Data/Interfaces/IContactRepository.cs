using System.Data;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Contacts;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.Models;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Data.Interfaces;

public interface IContactRepository : IEntityRepository<Contact>
{
    /// <summary>
    /// Header row for the contact profile page — identity, photo, composed display/preferred names.
    /// Returns null if no contact matches.
    /// </summary>
    Task<ContactHeaderRow?> GetHeaderByIdAsync(Guid contactId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    /// <summary>
    /// Basic-details area: person bio (no cultural/medical fields) + the Contact-row fields. Null if
    /// no match.
    /// </summary>
    Task<ContactBasicDetailsResponse?> GetBasicDetailsByIdAsync(Guid contactId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);

    /// <summary>
    /// Paged contact summary for the contact list / picker. Backed by an inner join on
    /// <c>Contacts</c> so people who only exist as staff/students are excluded.
    /// </summary>
    Task<PageResult<ContactSummaryResponse>> GetContactsAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The Contact id for a given person, or null if the person isn't (active) a contact. Used to
    /// block a duplicate contact role on the same Person.
    /// </summary>
    Task<Guid?> GetContactIdByPersonIdAsync(Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    /// <summary>
    /// Search existing People (any subtype) by name for the "new contact" create flow, so someone
    /// already on file gets a contact role attached to their existing Person rather than a duplicate.
    /// Each result carries <c>ExistingContactId</c> when the person is already (active) a contact.
    /// <paramref name="like"/> is the caller-built contains pattern. Capped at 25 rows.
    /// </summary>
    Task<IReadOnlyList<ContactMatchResponse>> SearchPeopleForContactCreateAsync(string like,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
