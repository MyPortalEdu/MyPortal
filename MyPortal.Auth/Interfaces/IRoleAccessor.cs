namespace MyPortal.Auth.Interfaces;

public interface IRoleAccessor
{
    Task<IReadOnlyCollection<Guid>> GetRolesForUserAsync(Guid userId, CancellationToken ct = default);
}