using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("BasketItems")]
    public class BasketItem : Entity
    {
        public Guid StudentId { get; set; }

        public Guid ProductId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        public Student? Student { get; set; }

        public Product? Product { get; set; }
    }
}