using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ObservationOutcomes")]
    public class ObservationOutcome : LookupEntity
    {
        [StringLength(128)]
        public string? ColourCode { get; set; }
    }
}