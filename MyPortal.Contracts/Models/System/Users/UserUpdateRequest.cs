using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Models.System.Users;

// Update payload — no Password. A user's password is changed through the dedicated set-password
// (admin) / change-password (self) endpoints, never as part of a general edit.
public class UserUpdateRequest
{
    public UserUpdateRequest()
    {
        RoleIds = new List<Guid>();
    }

    public Guid? PersonId { get; set; }

    public UserType UserType { get; set; }

    public bool IsEnabled { get; set; }

    public string Username { get; set; } = null!;

    public string? Email { get; set; }

    public IList<Guid> RoleIds { get; set; }
}
