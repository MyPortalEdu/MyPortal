using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // A pension scheme a contract can be enrolled in (TPS, LGPS, NEST, opted out).
    [Table("SuperannuationSchemes")]
    public class SuperannuationScheme : LookupEntity, ISystemEntity, IOrderedLookupEntity
    {
        [StringLength(10)]
        public string? Code { get; set; }

        public bool IsSystem { get; set; }

        public int DisplayOrder { get; set; }
    }
}
