using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // Outcome of an exclusion review (CBDS CS105).
    [Table("ExclusionAppealResults")]
    public class ExclusionAppealResult : LookupEntity, ISystemEntity, IOrderedLookupEntity
    {
        [StringLength(10)]
        public string? Code { get; set; }

        public bool IsSystem { get; set; }

        public int DisplayOrder { get; set; }
    }
}
