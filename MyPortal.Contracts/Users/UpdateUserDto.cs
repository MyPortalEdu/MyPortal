using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Users;

public class UpdateUserDto
{
    public UpdateUserDto()
    {
        RoleIds = new List<Guid>();
    }
    
    public Guid? PersonId { get; set; }
        
    public UserType UserType { get; set; }

    public bool IsEnabled { get; set; }
    
    public required string Username { get; set; }
    
    public string? Email { get; set; }
    
    public IList<Guid> RoleIds { get; set; }
}