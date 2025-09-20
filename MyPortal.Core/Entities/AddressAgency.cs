using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("AddressAgencies")]
public class AddressAgency : Entity
{
    public Guid AddressId { get; set; }

    public Guid AgencyId { get; set; }

    public Guid AddressTypeId { get; set; }

    public bool IsMain { get; set; }

    public Address? Address { get; set; }
    public Agency? Agency { get; set; }
    public AddressType? AddressType { get; set; }
}