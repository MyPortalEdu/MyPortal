using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ChargeDiscounts")]
    public class ChargeDiscount : Entity
    {
        public Guid ChargeId { get; set; }

        public Guid DiscountId { get; set; }

        public Charge? Charge { get; set; }
        public Discount? Discount { get; set; }
    }
}