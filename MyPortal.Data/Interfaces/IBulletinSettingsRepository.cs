using System.Data;
using MyPortal.Contracts.Models.Bulletins;

namespace MyPortal.Data.Interfaces;

public interface IBulletinSettingsRepository
{
    Task<IList<BulletinAllowedGroupResponse>> GetAllowedAudienceGroupsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Replaces the allowlist of student-group ids that may be picked as a bulletin audience.
    /// </summary>
    Task ReplaceAllowedAudienceGroupsAsync(IList<Guid> studentGroupIds, CancellationToken cancellationToken,
        IDbTransaction? transaction = null);
}
