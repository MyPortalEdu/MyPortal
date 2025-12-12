using Microsoft.AspNetCore.Identity;
using MyPortal.Contracts.Models.System.Users;
using QueryKit.Repositories.Filtering;
using QueryKit.Repositories.Paging;
using QueryKit.Repositories.Sorting;

namespace MyPortal.Services.Interfaces.Services;

/// <summary>
/// Defines operations for managing user accounts, including retrieving user information, creating, updating, deleting
/// users, and handling password changes.
/// </summary>
/// <remarks>This interface provides asynchronous methods for user management tasks, supporting filtering,
/// sorting, and paging when listing users. Implementations should ensure appropriate authorization and validation for
/// all operations. Methods accept a <see cref="CancellationToken"/> to support cancellation of long-running
/// operations.</remarks>
public interface IUserService
{
    /// <summary>
    /// Asynchronously retrieves detailed information for the specified user by unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose details are to be retrieved.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="UserDetailsResponse"/>
    /// with the user's details if found; otherwise, <see langword="null"/>.</returns>
    Task<UserDetailsResponse?> GetDetailsByIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves user information for the specified user identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose information is to be retrieved.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="UserInfoResponse"/>
    /// with the user's information if found; otherwise, <see langword="null"/>.</returns>
    Task<UserInfoResponse?> GetInfoByIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously retrieves a paged list of users matching the specified filter and sort criteria.
    /// </summary>
    /// <param name="filter">An optional set of filtering criteria to restrict which users are returned. If null, no filtering is applied.</param>
    /// <param name="sort">An optional set of sorting options that determines the order of the returned users. If null, the default sort
    /// order is used.</param>
    /// <param name="paging">An optional set of paging options that controls the number of users returned and the page to retrieve. If null,
    /// default paging is applied.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a paged collection of user summary
    /// responses matching the specified criteria. The collection may be empty if no users match.</returns>
    Task<PageResult<UserSummaryResponse>> GetUsersAsync(FilterOptions? filter = null,
        SortOptions? sort = null, PageOptions? paging = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes the password for the specified user asynchronously using the provided password change request.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose password will be changed.</param>
    /// <param name="model">An object containing the current and new password information required to perform the password change.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the password change operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IdentityResult indicating whether
    /// the password change was successful.</returns>
    Task<IdentityResult> ChangePasswordAsync(Guid userId, UserChangePasswordRequest model,
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Sets a new password for the specified user asynchronously.
    /// </summary>
    /// <remarks>This method does not validate the user's current password. Ensure that the password in the
    /// request meets any configured password policies. If the operation fails, the returned IdentityResult will contain
    /// error information.</remarks>
    /// <param name="userId">The unique identifier of the user whose password will be set.</param>
    /// <param name="model">An object containing the new password and any additional password requirements.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the password set operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IdentityResult indicating whether
    /// the password was set successfully.</returns>
    Task<IdentityResult> SetPasswordAsync(Guid userId, UserSetPasswordRequest model,  CancellationToken cancellationToken);
    
    /// <summary>
    /// Asynchronously creates a new user account using the specified user details.
    /// </summary>
    /// <remarks>If the user creation fails, the returned IdentityResult will contain error information
    /// describing the reason for failure.</remarks>
    /// <param name="model">An object containing the information required to create or update a user. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IdentityResult indicating whether
    /// the user creation succeeded or failed.</returns>
    Task<IdentityResult> CreateUserAsync(UserUpsertRequest model, CancellationToken cancellationToken);
    
    /// <summary>
    /// Asynchronously updates the user identified by the specified user ID with the provided data.
    /// </summary>
    /// <remarks>If the user does not exist, the operation will fail and the returned IdentityResult will
    /// contain error information. This method does not throw if the user is not found; check the result for success or
    /// failure.</remarks>
    /// <param name="userId">The unique identifier of the user to update.</param>
    /// <param name="model">An object containing the updated user information to apply. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the update operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an IdentityResult indicating whether
    /// the update succeeded and any associated errors.</returns>
    Task<IdentityResult> UpdateUserAsync(Guid userId, UserUpsertRequest model, CancellationToken cancellationToken);
    
    /// <summary>
    /// Asynchronously deletes the user identified by the specified unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to delete. Must correspond to an existing user.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>A task that represents the asynchronous delete operation. The task result contains an IdentityResult indicating
    /// whether the deletion succeeded.</returns>
    Task<IdentityResult> DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
}