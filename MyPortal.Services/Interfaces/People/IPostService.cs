using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

/// <summary>
/// Owns the established-posts register in Staff Setup — school-level reference data that contracts
/// are held against and vacancies are reported for. Gated on Staff.ViewStaffSetup / EditStaffSetup.
/// </summary>
public interface IPostService
{
    Task<PostsResponse> GetPostsAsync(CancellationToken cancellationToken);

    Task<Guid> CreatePostAsync(PostUpsertRequest model, CancellationToken cancellationToken);

    Task UpdatePostAsync(Guid id, PostUpsertRequest model, CancellationToken cancellationToken);

    Task DeletePostAsync(Guid id, CancellationToken cancellationToken);
}
