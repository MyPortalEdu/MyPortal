using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Database.Models.Entity
{
    [Table("building")]
    public class Building : BaseTypes.LookupItem
    {
        public virtual ICollection<BuildingFloor> Floors { get; set; }
    }
}