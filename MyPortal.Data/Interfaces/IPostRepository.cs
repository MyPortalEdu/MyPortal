using System.Data;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces.Base;
using MyPortal.Data.Models;

namespace MyPortal.Data.Interfaces;

public interface IPostRepository : IEntityRepository<Post>
{
    /// <summary>Every live post with its in-use contract count. Soft-deleted rows excluded.</summary>
    Task<IEnumerable<PostRow>> GetAllWithUsageAsync(CancellationToken cancellationToken,
        IDbTransaction? transaction = null);

    /// <summary>True when another live post already uses this reference.</summary>
    Task<bool> ReferenceExistsAsync(string reference, Guid? excludePostId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null);
}
