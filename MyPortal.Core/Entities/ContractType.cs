using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("ContractTypes")]
    public class ContractType : LookupEntity, IOrderedLookupEntity
    {
        // DfE CBDS Contract Type code (e.g. PRM, FXT, TMP) — for workforce-census mapping.
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
