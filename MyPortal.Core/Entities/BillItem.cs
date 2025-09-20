using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("BillItems")]
    public class BillItem : Entity
    {
        public Guid BillId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
        
        public decimal NetAmount { get; set; }
        
        public decimal VatAmount { get; set; }

        public bool CustomerReceived { get; set; }

        public bool IsRefunded { get; set; }

        public Bill? Bill { get; set; }
        public Product? Product { get; set; }
    }
}