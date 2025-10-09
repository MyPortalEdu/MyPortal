using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Models.System.Users;

public class UserDetailsDto
{
    public UserDetailsDto()
    {
        RoleIds =  new List<Guid>();
    }
    
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public Guid? PersonId { get; set; }
        
    public UserType UserType { get; set; }

    public bool IsEnabled { get; set; }

    public string? PersonFullName { get; set; }
        
    // Identity
    public string? Username { get; set; }
    
    public string? NormalizedUsername { get; set; }
    
    public string? Email { get; set; }
    
    public string? NormalizedEmail { get; set; }

    public bool EmailConfirmed { get; set; }

    public string? SecurityStamp { get; set; }

    public string? ConcurrencyStamp { get; set; }
    
    public string? PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public DateTime? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public int AccessFailedCount { get; set; }

    public IList<Guid> RoleIds { get; set; }
}