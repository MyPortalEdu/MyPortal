using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("AcademicTerms")]
    public class AcademicTerm : Entity, IAuditableEntity, IAcademicYearEntity, IVersionedEntity
    {
        public Guid AcademicYearId { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; } = null!;   

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public AcademicYear? AcademicYear { get; set; }
        
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