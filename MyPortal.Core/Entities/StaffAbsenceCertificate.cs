using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    // A certificate covering (part of) an absence — a self-certification form, a doctor's fit note,
    // or a return-to-work record. One absence can accumulate several. Lean like its parent
    // StaffAbsence: no audit / soft-delete.
    [Table("StaffAbsenceCertificates")]
    public class StaffAbsenceCertificate : Entity
    {
        public Guid StaffAbsenceId { get; set; }

        public DateTime DateReceived { get; set; }

        public DateTime? DateSigned { get; set; }

        // Self-certified by the staff member, rather than signed by a doctor. Drives the
        // self-certified vs medically-certified split in statutory absence reporting.
        public bool IsSelfCertified { get; set; }

        // A return-to-work record rather than a sickness certificate.
        public bool IsReturnToWork { get; set; }

        // Signing doctor / practice (blank when self-certified).
        [StringLength(256)]
        public string? SignedBy { get; set; }

        [StringLength(256)]
        public string? Notes { get; set; }

        public StaffAbsence? StaffAbsence { get; set; }
    }
}
