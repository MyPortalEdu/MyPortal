using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("BillDiscounts")]
    public class BillDiscount : Entity
    {
        public Guid BillId { get; set; }

        public Guid DiscountId { get; set; }
        
        public decimal GrossAmount { get; set; }

        public Bill? Bill { get; set; }
        public Discount? Discount { get; set; }
    }
}