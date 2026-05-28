using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Nationalities")]
    public class Nationality : LookupEntity
    {
        [StringLength(3)]
        public string? IsoCode { get; set; }
    }
}
