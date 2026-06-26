using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Religions")]
    public class Religion : LookupEntity, IOrderedLookupEntity
    {
        // DfE CBDS "Faith" code (e.g. CE, MU, SI) — for statutory/return mapping.
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
