using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Incidents")]
    public class Incident : Entity, IAuditableEntity, ISoftDeleteEntity, IAcademicYearEntity, IVersionedEntity
    {
        public Guid AcademicYearId { get; set; }

        public Guid IncidentTypeId { get; set; }

        public Guid? LocationId { get; set; }
        
        public DateTime Date { get; set; }

        public string? Comments { get; set; }

        public bool IsDeleted { get; set; }

        public IncidentType? IncidentType { get; set; }

        public Location? Location { get; set; }

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