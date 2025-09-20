using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("AccountTransactions")]
    public class AccountTransaction : Entity
    {
        public Guid StudentId { get; set; }
        
        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public Student? Student { get; set; }
    }
}