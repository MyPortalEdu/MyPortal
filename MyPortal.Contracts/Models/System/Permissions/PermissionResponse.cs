using System.ComponentModel.DataAnnotations;

namespace MyPortal.Contracts.Models.System.Permissions
{
    public class PermissionResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string FriendlyName { get; set; } = null!;

        public string Area { get; set; } = null!;
    }
}
