using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Database.BaseTypes;
using MyPortal.Database.Interfaces;

namespace MyPortal.Database.Models.Entity
{
    [Table("AgencyTypes")]
    public class AgencyType : LookupItem, ISystemEntity
    {
        public AgencyType()
        {
            Agencies = new HashSet<Agency>();
        }

        [Column(Order = 4)]
        public bool System { get; set; }

        public virtual ICollection<Agency> Agencies { get; set; }
    }
}