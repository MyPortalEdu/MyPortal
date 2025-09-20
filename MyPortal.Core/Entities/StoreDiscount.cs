using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("StoreDiscounts")]
    public class StoreDiscount : Entity
    {
        public Guid? ProductId { get; set; }

        public Guid? ProductTypeId { get; set; }

        public Guid DiscountId { get; set; }

        // Apply the discount to the cart total
        public bool IsAppliedToCart { get; set; }

        // How many of this product in the basket should the discount apply to
        // E.g. If the user has 3 apples in their basket, but you only want to apply discount to 1 of them
        // For example, in a BOGOF situaton where you apply 100% discount to one of the 2 items
        // null for all
        public int? ApplyTo { get; set; }

        public Product? Product { get; set; }
        public ProductType? ProductType { get; set; }
        public Discount? Discount { get; set; }
    }
}