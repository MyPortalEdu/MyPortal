using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("TrainingCertificateStatus")]
    public class TrainingCertificateStatus : LookupEntity
    {
        [StringLength(128)]
        public string? ColourCode { get; set; }
    }
}