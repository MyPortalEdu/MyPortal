using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("VatRates")]
    public class VatRate : LookupEntity
    {
        public decimal Value { get; set; }
    }
}