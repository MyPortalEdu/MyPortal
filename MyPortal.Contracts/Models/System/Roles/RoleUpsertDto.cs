
namespace MyPortal.Contracts.Models.System.Roles
{
    public class RoleUpsertDto
    {
        public RoleUpsertDto()
        {
            PermissionIds = new List<Guid>();
        }
        
        public string? Description { get; set; }

        public string? Name { get; set; }

        public IList<Guid> PermissionIds { get; set; }
    }
}
