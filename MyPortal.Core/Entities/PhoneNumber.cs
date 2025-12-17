using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("PhoneNumbers")]
    public class PhoneNumber : Entity
    {
        public Guid TypeId { get; set; }

        public Guid? PersonId { get; set; }

        public Guid? AgencyId { get; set; }

        [Phone]
        [Required]
        [StringLength(128)]
        public string Number { get; set; } = null!;

        public bool IsMain { get; set; }

        public PhoneNumberType? Type { get; set; }
        public Person? Person { get; set; }
        public Agency? Agency { get; set; }
    }
}