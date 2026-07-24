using MyPortal.Contracts.Models.People;

namespace MyPortal.Services.Interfaces.People;

public interface IPostService
{
    Task<PostsResponse> GetPostsAsync(CancellationToken cancellationToken);

    Task<Guid> CreatePostAsync(PostUpsertRequest model, CancellationToken cancellationToken);

    Task UpdatePostAsync(Guid id, PostUpsertRequest model, CancellationToken cancellationToken);

    Task DeletePostAsync(Guid id, CancellationToken cancellationToken);
}
