using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("ReportCards")]
    public class ReportCard : Entity, IAcademicYearEntity
    {
        public Guid StudentId { get; set; }

        public Guid AcademicYearId { get; set; }

        public Guid BehaviourTypeId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
        
        [StringLength(256)]
        public string? Comments { get; set; }

        public bool IsActive { get; set; }

        public Student? Student { get; set; }
        public AcademicYear? AcademicYear { get; set; }
        public IncidentType? BehaviourType { get; set; }
    }
}