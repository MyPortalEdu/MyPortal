using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("Permissions")]
public class Permission : Entity
{
    [Required, StringLength(50)]
    public required string Name { get; set; }

    [Required, StringLength(100)]
    public required string FriendlyName { get; set; }

    [Required, StringLength(50)]
    public required string Area { get; set; }
}