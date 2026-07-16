using MyPortal.Common.Enums;

namespace MyPortal.Auth.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }

    /// <summary>The Person the user is linked to, or null for users with no person identity.</summary>
    Guid? PersonId { get; }

    string? IpAddress { get; }
    UserType UserType { get; }
    Task<IReadOnlyCollection<Guid>> GetRolesAsync(CancellationToken ct = default);
}