using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Database.BaseTypes;

namespace MyPortal.Database.Models.Entity;

[Table("behaviour_role_type")]
public class BehaviourRoleType : LookupItem
{
    [Column(Order = 4)] public int DefaultPoints { get; set; }

    public virtual ICollection<StudentIncident> LinkedIncidents { get; set; }
}