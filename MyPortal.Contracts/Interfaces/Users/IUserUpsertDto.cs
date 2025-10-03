using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Interfaces.Users;

public interface IUserUpsertDto
{
    Guid? PersonId { get; }
    UserType UserType { get; }
    bool IsEnabled { get; }
    string Username { get; }
    string? Email { get; }
    IList<Guid> RoleIds { get; }
}