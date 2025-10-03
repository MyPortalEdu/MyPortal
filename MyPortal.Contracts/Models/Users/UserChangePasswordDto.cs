namespace MyPortal.Contracts.Models.Users;

public class UserChangePasswordDto
{
    public Guid UserId { get; set; }
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}