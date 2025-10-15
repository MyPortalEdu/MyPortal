using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Interfaces.System.Users;

public interface IUpsertUserDto
{
    Guid? PersonId { get; }
    UserType UserType { get; }
    bool IsEnabled { get; }
    string Username { get; }
    string? Email { get; }
    IList<Guid> RoleIds { get; }
}