using MyPortal.Common.Enums;
using MyPortal.Contracts.Interfaces.System.Users;

namespace MyPortal.Contracts.Models.System.Users;

public class CreateUpsertUserDto : IUpsertUserDto, IUserPasswordDto
{
    public CreateUpsertUserDto()
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