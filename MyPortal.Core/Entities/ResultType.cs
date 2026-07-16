using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // Nature of a result (CBDS CS014) — Estimate (E), Interim (I), Provisional
    // (P), Result (R), Target (T).
    [Table("ResultTypes")]
    public class ResultType : LookupEntity, IOrderedLookupEntity
    {
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
