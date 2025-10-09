using MyPortal.Common.Enums;
using MyPortal.Contracts.Interfaces;
using MyPortal.Contracts.Interfaces.Users;

namespace MyPortal.Contracts.Models.System.Users;

public class UpdateUserDto : IUserUpsertDto, IUpdateDto
{
    public UpdateUserDto()
    {
        RoleIds = new List<Guid>();
    }
    
    public Guid Id { get; set; }
    
    public Guid? PersonId { get; set; }
        
    public UserType UserType { get; set; }

    public bool IsEnabled { get; set; }
    
    public required string Username { get; set; }
    
    public string? Email { get; set; }
    
    public IList<Guid> RoleIds { get; set; }
}