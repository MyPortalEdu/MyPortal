using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("RolePermissions")]
public class RolePermission : Entity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    
    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
}