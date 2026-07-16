using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    // Staff additional / supplementary payment type (CBDS CS082) — e.g. TLR1/2
    // (TLE), TLR3 (TL3), SEN Allowance (SEN), London Weighting (LIN/LOT/LFR).
    [Table("AdditionalPaymentTypes")]
    public class AdditionalPaymentType : LookupEntity, IOrderedLookupEntity
    {
        [StringLength(10)]
        public string? Code { get; set; }

        public int DisplayOrder { get; set; }
    }
}
