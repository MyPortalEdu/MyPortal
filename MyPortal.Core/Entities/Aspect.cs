using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("Aspects")]
    public class Aspect : LookupEntity, ISystemEntity
    {
        public Guid TypeId { get; set; }

        public Guid? GradeSetId { get; set; }
        
        public decimal? MinMark { get; set; }
        
        public decimal? MaxMark { get; set; }
        
        [Required, StringLength(128)]
        public required string Name { get; set; }
        
        [Required, StringLength(50)]
        public required string ColumnHeading { get; set; }

        // Only visible to staff users
        public bool IsPrivate { get; set; }

        public bool IsSystem { get; set; }

        public AspectType? Type { get; set; }

        public GradeSet? GradeSet { get; set; }
    }
}