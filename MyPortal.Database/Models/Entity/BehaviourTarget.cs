using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Database.BaseTypes;

namespace MyPortal.Database.Models.Entity
{
    [Table("behaviour_target")]
    public class BehaviourTarget : LookupItem
    {
        public virtual ICollection<ReportCardTarget> ReportCardLinks { get; set; }
    }
}