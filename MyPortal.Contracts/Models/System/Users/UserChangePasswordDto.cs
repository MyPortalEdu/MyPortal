namespace MyPortal.Contracts.Models.System.Users;

public class UserChangePasswordDto
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}