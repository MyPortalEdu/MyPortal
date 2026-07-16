using MyPortal.Common.Enums;

namespace MyPortal.Contracts.Models.System.Roles
{
    public class RoleUpsertRequest
    {
        public RoleUpsertRequest()
        {
            PermissionIds = new List<Guid>();
        }

        public string? Description { get; set; }

        public string? Name { get; set; }

        // Portal audience. Set on create; ignored on update (a role's audience is immutable).
        public UserType UserType { get; set; }

        public IList<Guid> PermissionIds { get; set; }
    }
}
