using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("BillAccountTransactions")]
    public class BillAccountTransaction : Entity
    {
        public Guid BillId { get; set; }

        public Guid AccountTransactionId { get; set; }

        public Bill? Bill { get; set; }
        public AccountTransaction? AccountTransaction { get; set; }
    }
}