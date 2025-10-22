using MyPortal.Common.Enums;
using MyPortal.Contracts.Interfaces.System.Users;

namespace MyPortal.Contracts.Models.System.Users;

public class UserUpsertDto : IUserPasswordDto
{
    public UserUpsertDto()
    {
        RoleIds = new List<Guid>();
    }
    
    public Guid? PersonId { get; set; }
        
    public UserType UserType { get; set; }

    public bool IsEnabled { get; set; }
    
    public required string Username { get; set; }
    
    public string? Email { get; set; }

    // Only used for user creation
    public string? Password { get; set; }
    
    public IList<Guid> RoleIds { get; set; }
}