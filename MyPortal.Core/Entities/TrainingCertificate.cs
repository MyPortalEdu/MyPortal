using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("TrainingCertificates")]
    public class TrainingCertificate : Entity
    {
        public Guid TrainingCourseId { get; set; }

        // Set when this record is an attendee booking on a scheduled training event.
        public Guid? TrainingEventId { get; set; }

        public Guid StaffMemberId { get; set; }

        public Guid TrainingCertificateStatusId { get; set; }

        public DateTime? CompletedDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        [StringLength(128)]
        public string? Provider { get; set; }

        // CPD hours / points.
        public decimal? Hours { get; set; }

        [StringLength(64)]
        public string? CertificateReference { get; set; }

        public StaffMember? StaffMember { get; set; }
        public TrainingCourse? TrainingCourse { get; set; }
        public TrainingCertificateStatus? Status { get; set; }
    }
}