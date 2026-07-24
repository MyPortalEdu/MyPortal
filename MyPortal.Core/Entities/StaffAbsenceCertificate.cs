using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("StaffAbsenceCertificates")]
    public class StaffAbsenceCertificate : Entity
    {
        public Guid StaffAbsenceId { get; set; }

        public DateTime DateReceived { get; set; }

        public DateTime? DateSigned { get; set; }

        public bool IsSelfCertified { get; set; }

        public bool IsReturnToWork { get; set; }

        [StringLength(256)]
        public string? SignedBy { get; set; }

        [StringLength(256)]
        public string? Notes { get; set; }

        public StaffAbsence? StaffAbsence { get; set; }
    }
}
