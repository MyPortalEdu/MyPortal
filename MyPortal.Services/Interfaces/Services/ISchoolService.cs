using MyPortal.Contracts.Models.School;

namespace MyPortal.Services.Interfaces.Services;

/// <summary>
/// Defines methods for retrieving school details, including information about the local school and schools by unique
/// identifier.
/// </summary>
/// <remarks>Implementations of this interface should ensure that returned data accurately reflects the current
/// state of the school records. Methods are asynchronous and support cancellation via the provided token.</remarks>
public interface ISchoolService
{
    /// <summary>
    /// Asynchronously retrieves details about the local school associated with the current context.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="SchoolDetailsResponse"/> with information about the local school, or <see langword="null"/> if no school
    /// is found.</returns>
    Task<SchoolDetailsResponse?> GetLocalSchoolAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Asynchronously retrieves detailed information about a school by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the school to retrieve.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see
    /// cref="SchoolDetailsResponse"/> with the school's details if found; otherwise, <see langword="null"/>.</returns>
    Task<SchoolDetailsResponse?> GetSchoolByIdAsync(Guid id, CancellationToken cancellationToken);
}