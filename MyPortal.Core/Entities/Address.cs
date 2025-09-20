using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Addresses")]
    public class Address : Entity
    {
        [StringLength(128)]
        public string? BuildingNumber { get; set; }
        
        [StringLength(128)]
        public string? BuildingName { get; set; }
        
        [StringLength(128)]
        public string? Apartment { get; set; }
        
        [Required]
        [StringLength(256)]
        public required string Street { get; set; }
        
        [StringLength(256)]
        public string? District { get; set; }
        
        [Required, StringLength(256)]
        public required string Town { get; set; }
        
        [Required, StringLength(256)]
        public required string County { get; set; }

        [Required, StringLength(128)]
        public required string Postcode { get; set; }
        
        [Required, StringLength(128)]
        public required string Country { get; set; }

        public bool IsValidated { get; set; }
    }
}