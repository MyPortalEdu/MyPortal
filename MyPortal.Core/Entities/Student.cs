using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Students")]
    public class Student : Entity, IAuditableEntity, ISoftDeleteEntity, IVersionedEntity
    {
        public Guid PersonId { get; set; }

        public int AdmissionNumber { get; set; }

        public DateTime? DateStarting { get; set; }

        public DateTime? DateLeaving { get; set; }

        public bool FreeSchoolMeals { get; set; }

        public Guid? SenStatusId { get; set; }

        public Guid? SenTypeId { get; set; }

        public Guid? EnrolmentStatusId { get; set; }

        public Guid? BoarderStatusId { get; set; }

        public bool PupilPremium { get; set; }
        
        [StringLength(13)]
        public string? Upn { get; set; }

        public bool IsDeleted { get; set; }

        public Person? Person { get; set; }
        public SenStatus? SenStatus { get; set; }
        public SenType? SenType { get; set; }
        public EnrolmentStatus? EnrolmentStatus { get; set; }
        public BoarderStatus? BoarderStatus { get; set; }
        
        // Audit
        public Guid CreatedById { get; set; }
        public string CreatedByIpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid LastModifiedById { get; set; }
        public string LastModifiedByIpAddress { get; set; }
        public DateTime LastModifiedAt { get; set; }
        public User? CreatedBy { get; set; }
        public User? LastModifiedBy { get; set; }
        public long Version { get; set; }
    }
}