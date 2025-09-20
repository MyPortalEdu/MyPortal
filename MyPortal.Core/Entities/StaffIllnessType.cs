using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("StaffIllnessTypes")]
    public class StaffIllnessType : LookupEntity, ISystemEntity
    {
        public bool IsSystem { get; set; }
    }
}