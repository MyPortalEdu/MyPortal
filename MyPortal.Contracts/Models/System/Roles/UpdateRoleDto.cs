using MyPortal.Contracts.Interfaces;
using MyPortal.Contracts.Interfaces.System.Roles;

namespace MyPortal.Contracts.Models.System.Roles
{
    public class UpdateRoleDto : IUpsertRoleDto, IUpdateDto
    {
        public Guid Id { get; set; }

        public string? Description { get; set; }

        public string? Name { get; set; }

        public IList<Guid> PermissionIds { get; set; }
    }
}
