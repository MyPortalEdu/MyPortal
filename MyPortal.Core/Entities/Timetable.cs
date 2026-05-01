using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Enums;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities;

[Table("Timetables")]
public class Timetable : Entity, IAuditableEntity, IAcademicYearEntity, IVersionedEntity
{
    public Guid AcademicYearId { get; set; }

    [Required]
    [StringLength(128)]
    public string Name { get; set; } = null!;

    public TimetableStatus Status { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public AcademicYear? AcademicYear { get; set; }

    // Audit
    public Guid CreatedById { get; set; }
    public string CreatedByIpAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid LastModifiedById { get; set; }
    public string LastModifiedByIpAddress { get; set; } = string.Empty;
    public DateTime LastModifiedAt { get; set; }
    public User? CreatedBy { get; set; }
    public User? LastModifiedBy { get; set; }
    public long Version { get; set; }
}
