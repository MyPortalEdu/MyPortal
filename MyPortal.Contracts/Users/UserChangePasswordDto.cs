namespace MyPortal.Contracts.Users;

public class UserChangePasswordDto
{
    public Guid UserId { get; set; }
    public required string NewPassword { get; set; }
}