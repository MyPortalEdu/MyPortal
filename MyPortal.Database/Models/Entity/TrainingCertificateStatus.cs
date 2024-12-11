using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Database.BaseTypes;

namespace MyPortal.Database.Models.Entity
{
    [Table("training_certificate_status")]
    public class TrainingCertificateStatus : LookupItem
    {
        public TrainingCertificateStatus()
        {
            Certificates = new HashSet<TrainingCertificate>();
        }

        [Column(Order = 4)]
        [StringLength(128)]
        public string ColourCode { get; set; }

        public virtual ICollection<TrainingCertificate> Certificates { get; set; }
    }
}