using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("AddressPeople")]
    public class AddressPerson : Entity
    {
        public Guid AddressId { get; set; }

        public Guid? PersonId { get; set; }

        public Guid AddressTypeId { get; set; }

        public bool IsMain { get; set; }

        public Address? Address { get; set; }
        public Person? Person { get; set; }
        public AddressType? AddressType { get; set; }
    }
}