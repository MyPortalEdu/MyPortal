using Microsoft.AspNetCore.Identity;
using MyPortal.Contracts.Models.System.Roles;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Services;

/// <summary>
/// Defines operations for managing roles, including retrieval, creation, updating, and deletion of role entities.
/// </summary>
/// <remarks>This interface provides asynchronous methods for working with roles in an identity or authorization
/// system. Implementations should ensure thread safety and handle cancellation requests appropriately. Methods support
/// filtering, sorting, and paging for role queries, and return detailed or summary information as needed.</remarks>
public interface IRoleService
{
    /// <summary>
    /// Asynchronously retrieves detailed information for the specified role identifier.
    /// </summary>
    /// <param name="roleId">The unique identifier of the role to retrieve details for.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="RoleDetailsResponse"/>
    /// with the role details if found; otherwise, <see langword="null"/>.</returns>
    Task<RoleDetailsResponse?> GetDetailsByIdAsync(Guid roleId, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a paged list of role summaries, optionally filtered and sorted according to the
    /// specified criteria.
    /// </summary>
    /// <param name="filter">An optional set of filter criteria to restrict which roles are included in the result. If null, no filtering is
    /// applied.</param>
    /// <param name="sort">An optional set of sort options that determines the order of the returned roles. If null, the default sort order
    /// is used.</param>
    /// <param name="paging">An optional set of paging options that controls the number of roles returned and the page to retrieve. If null,
    /// default paging is applied.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paged collection of role summaries
    /// matching the specified criteria.</returns>
    Task<PageResult<RoleSummaryResponse>> GetRolesAsync(FilterOptions? filter = null, SortOptions? sort = null,
        PageOptions? paging = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously creates a new role using the specified role data.
    /// </summary>
    /// <param name="model">The role information to create. Must not be null. The properties of this object define the name and attributes
    /// of the new role.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IdentityResult indicating whether
    /// the role creation succeeded.</returns>
    Task<IdentityResult> CreateRoleAsync(RoleUpsertRequest model, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously updates the details of an existing role identified by the specified ID.
    /// </summary>
    /// <remarks>If the specified role does not exist, the operation will fail. The update is performed
    /// atomically and may fail if validation errors occur.</remarks>
    /// <param name="roleId">The unique identifier of the role to update.</param>
    /// <param name="model">An object containing the updated role information to apply. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IdentityResult indicating whether
    /// the update succeeded.</returns>
    Task<IdentityResult> UpdateRoleAsync(Guid roleId, RoleUpsertRequest model, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the role identified by the specified unique identifier asynchronously.
    /// </summary>
    /// <remarks>If the specified role does not exist, the operation will fail and the returned IdentityResult
    /// will contain error information. This method does not throw if the role is not found; check the result for
    /// success or failure.</remarks>
    /// <param name="roleId">The unique identifier of the role to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IdentityResult indicating whether
    /// the deletion succeeded.</returns>
    Task<IdentityResult> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken);
}