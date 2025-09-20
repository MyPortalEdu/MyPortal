using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("TrainingCertificates")]
    public class TrainingCertificate : Entity
    {
        public Guid TrainingCourseId { get; set; }

        public Guid StaffMemberId { get; set; }

        public Guid TrainingCertificateStatusId { get; set; }

        public StaffMember? StaffMember { get; set; }
        public TrainingCourse? TrainingCourse { get; set; }
        public TrainingCertificateStatus? Status { get; set; }
    }
}