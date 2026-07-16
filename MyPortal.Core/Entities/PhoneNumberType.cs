using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("PhoneNumberTypes")]
    public class PhoneNumberType : LookupEntity, IOrderedLookupEntity
    {
        // DfE CBDS Telephone Type code (e.g. H, M, W).
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
