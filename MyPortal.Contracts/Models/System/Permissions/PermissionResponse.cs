using System.ComponentModel.DataAnnotations;

namespace MyPortal.Contracts.Models.System.Permissions
{
    public class PermissionResponse
    {
        public Guid Id { get; set; }

        public required string Name { get; set; }

        public required string FriendlyName { get; set; }

        public required string Area { get; set; }
    }
}
