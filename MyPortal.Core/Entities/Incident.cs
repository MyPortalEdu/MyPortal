using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Incidents")]
    public class Incident : AuditableEntity, ISoftDeleteEntity
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
    }
}