using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("ChargeBillingPeriods")]
public class ChargeBillingPeriod : Entity
{
    [Required]
    [StringLength(128)]
    public required string Name { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}