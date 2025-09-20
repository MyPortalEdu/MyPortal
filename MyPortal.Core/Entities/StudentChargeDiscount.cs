using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("StudentChargeDiscounts")]
    public class StudentChargeDiscount : Entity
    {
        public Guid StudentId { get; set; }

        public Guid ChargeDiscountId { get; set; }

        public Student? Student { get; set; }
        public ChargeDiscount? ChargeDiscount { get; set; }
    }
}