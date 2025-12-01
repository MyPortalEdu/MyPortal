using MyPortal.Contracts.Interfaces.System.Users;

namespace MyPortal.Contracts.Models.System.Users;

public class UserSetPasswordRequest : IUserPasswordRequest
{
    public string Password { get; set; } = null!;
}