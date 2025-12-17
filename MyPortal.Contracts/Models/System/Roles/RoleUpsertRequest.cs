
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

        public IList<Guid> PermissionIds { get; set; }
    }
}
