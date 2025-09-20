using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Results")]
    public class Result : AuditableEntity
    {
        public Guid ResultSetId { get; set; }

        public Guid StudentId { get; set; }

        public Guid AspectId { get; set; }

        public DateTime Date { get; set; }

        public Guid? GradeId { get; set; }
        
        public decimal? Mark { get; set; }

        // Used for comment result types
        [StringLength(1000)]
        public string? Comment { get; set; }

        public string? ColourCode { get; set; }

        // Used to add notes/comments to results
        public string? Note { get; set; }
        
        public ResultSet? ResultSet { get; set; }
        public Aspect? Aspect { get; set; }
        public Student? Student { get; set; }
        public Grade? Grade { get; set; }
    }
}