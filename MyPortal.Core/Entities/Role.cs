using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Roles")]
    public class Role : Entity, ISystemEntity
    {
        public string? Description { get; set; }
        public bool IsSystem { get; set; }
    }
}