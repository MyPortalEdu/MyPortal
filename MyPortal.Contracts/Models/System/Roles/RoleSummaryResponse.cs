using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Models.System.Roles
{
    public class RoleSummaryResponse
    {
        public Guid Id { get; set; }

        public string? Description { get; set; }

        public bool IsSystem { get; set; }

        public bool IsDefault { get; set; }

        public UserType UserType { get; set; }

        public string? Name { get; set; }
    }
}
