using MyPortal.Contracts.Interfaces.System.Users;

namespace MyPortal.Contracts.Models.System.Users;

public class UserChangePasswordRequest : IUserPasswordRequest
{
    public required string CurrentPassword { get; set; }
    public required string Password { get; set; }
}