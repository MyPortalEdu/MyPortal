using MyPortal.Contracts.Interfaces.System.Roles;

namespace MyPortal.Contracts.Models.System.Roles
{
    public class CreateRoleDto : IUpsertRoleDto
    {
        public string? Description { get; set; }

        public string? Name { get; set; }

        public IList<Guid> PermissionIds { get; set; }
    }
}
