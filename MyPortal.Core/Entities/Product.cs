using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Products")]
    public class Product : Entity, ISoftDeleteEntity
    {
        public Guid ProductTypeId { get; set; }

        public Guid VatRateId { get; set; }
        
        [Required]
        [StringLength(128)]
        public required string Name { get; set; }
        
        [Required]
        [StringLength(256)]
        public required string Description { get; set; }
        
        public decimal Price { get; set; }

        public bool ShowOnStore { get; set; }

        public int OrderLimit { get; set; }

        public bool IsDeleted { get; set; }

        public ProductType? Type { get; set; }

        public VatRate? VatRate { get; set; }
    }
}