using MyPortal.Contracts.Interfaces.System.Users;

namespace MyPortal.Contracts.Models.System.Users;

public class UserSetPasswordRequest : IUserPasswordRequest
{
    public required string Password { get; set; }
}