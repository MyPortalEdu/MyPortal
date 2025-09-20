using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("BehaviourRoleTypes")]
public class BehaviourRoleType : LookupEntity
{
    [Range(0, int.MaxValue)]
    public int DefaultPoints { get; set; }
}