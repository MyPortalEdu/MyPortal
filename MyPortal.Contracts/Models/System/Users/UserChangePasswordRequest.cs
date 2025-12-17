using MyPortal.Contracts.Interfaces.System.Users;

namespace MyPortal.Contracts.Models.System.Users;

public class UserChangePasswordRequest : IUserPasswordRequest
{
    public string CurrentPassword { get; set; } = null!;
    public string Password { get; set; } = null!;
}