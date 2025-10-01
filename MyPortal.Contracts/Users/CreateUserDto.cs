using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Users;

public class CreateUserDto
{
    public CreateUserDto()
    {
        RoleIds = new List<Guid>();
    }
    
    public Guid? PersonId { get; set; }
        
    public UserType UserType { get; set; }

    public bool IsEnabled { get; set; }
    
    public required string Username { get; set; }
    
    public string? Email { get; set; }

    public required string Password { get; set; }
    
    public IList<Guid> RoleIds { get; set; }
}