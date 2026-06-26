using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // Result type/format of an exam component, e.g. grade / mark / IB level (CBDS CS094).
    [Table("ExamComponentResultTypes")]
    public class ExamComponentResultType : LookupEntity, IOrderedLookupEntity
    {
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
