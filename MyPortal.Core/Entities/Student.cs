using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Students")]
    public class Student : Entity, ISoftDeleteEntity
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
    }
}