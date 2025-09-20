using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("EmailAddresses")]
    public class EmailAddress : Entity
    {
        public Guid TypeId { get; set; }

        public Guid? PersonId { get; set; }

        public Guid? AgencyId { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(128)]
        public required string Address { get; set; }

        public bool IsMain { get; set; }

        public string? Notes { get; set; }

        public Agency? Agency { get; set; }
        public Person? Person { get; set; }
        public EmailAddressType? Type { get; set; }
    }
}