using MyPortal.Common.Enums;

namespace MyPortal.Auth.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    UserType UserType { get; }
    Task<IReadOnlyCollection<Guid>> GetRolesAsync(CancellationToken ct = default);
}