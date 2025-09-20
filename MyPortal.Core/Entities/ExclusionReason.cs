using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // FIXED
    [Table("ExclusionReasons")]
    public class ExclusionReason : LookupEntity, ISystemEntity
    {
        public string? Code { get; set; }

        public bool IsSystem { get; set; }
    }
}