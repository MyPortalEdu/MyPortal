using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("AgencyTypes")]
    public class AgencyType : LookupEntity, ISystemEntity
    {
        public bool IsSystem { get; set; }
    }
}