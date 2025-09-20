using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("BillCharges")]
    public class BillStudentCharge : Entity
    {
        public Guid BillId { get; set; }

        public Guid StudentChargeId { get; set; }
        
        public decimal NetAmount { get; set; }
        
        public decimal VatAmount { get; set; }
        
        public Bill? Bill { get; set; }
        public StudentCharge? StudentCharge { get; set; }
    }
}