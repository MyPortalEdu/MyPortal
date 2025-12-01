using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("Permissions")]
public class Permission : Entity
{
    [Required, StringLength(50)]
    public string Name { get; set; } = null!;

    [Required, StringLength(100)]
    public string FriendlyName { get; set; } = null!;

    [Required, StringLength(50)]
    public string Area { get; set; } = null!;
}