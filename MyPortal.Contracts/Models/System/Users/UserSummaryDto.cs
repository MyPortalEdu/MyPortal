using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Models.System.Users;

public class UserSummaryDto
{
    public Guid Id { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public Guid? PersonId { get; set; }
        
    public UserType UserType { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsSystem { get; set; }

    public string? PersonFullName { get; set; }
        
    // Identity
    public string? Username { get; set; }
    
    public string? Email { get; set; }
    
    public string? PhoneNumber { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public bool LockoutEnabled { get; set; }
}