namespace MyPortal.Contracts.Models.Users;

public class UserSetPasswordDto
{
    public Guid UserId { get; set; }
    public required string NewPassword { get; set; }
}