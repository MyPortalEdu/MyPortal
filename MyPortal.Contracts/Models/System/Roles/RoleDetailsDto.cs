using System.ComponentModel.DataAnnotations;

namespace MyPortal.Contracts.Models.System.Roles
{
    public class RoleDetailsDto
    {
        public Guid Id { get; set; }
        
        public string? Description { get; set; }
        
        public bool IsSystem { get; set; }

        public string? Name { get; set; }
    }
}
