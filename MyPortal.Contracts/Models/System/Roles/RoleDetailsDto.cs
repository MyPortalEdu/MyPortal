using System.ComponentModel.DataAnnotations;

namespace MyPortal.Contracts.Models.System.Roles
{
    public class RoleDetailsDto
    {
        public string? Description { get; set; }
        
        public bool IsSystem { get; set; }

        public string? Name { get; set; }

        public string? NormalizedName { get; set; }

        public string? ConcurrencyStamp { get; set; }
    }
}
