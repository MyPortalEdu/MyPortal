using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Results")]
    public class Result : Entity, IAuditableEntity, IVersionedEntity
    {
        public Guid ResultSetId { get; set; }

        public Guid StudentId { get; set; }

        public Guid AspectId { get; set; }

        public DateTime Date { get; set; }

        public Guid? GradeId { get; set; }

        // Nature of the result — Estimate / Interim / Provisional / Result / Target (CBDS CS014).
        public Guid? ResultTypeId { get; set; }

        public decimal? Mark { get; set; }

        // Used for comment result types
        [StringLength(1000)]
        public string? Comment { get; set; }

        public string? ColourCode { get; set; }

        // Used to add notes/comments to results
        public string? Note { get; set; }
        
        public ResultSet? ResultSet { get; set; }
        public ResultType? ResultType { get; set; }
        public Aspect? Aspect { get; set; }
        public Student? Student { get; set; }
        public Grade? Grade { get; set; }
        
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
}