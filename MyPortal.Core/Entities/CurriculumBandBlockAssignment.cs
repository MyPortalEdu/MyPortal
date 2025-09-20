using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("CurriculumBandBlockAssignments")]
    public class CurriculumBandBlockAssignment : Entity
    {
        public Guid BlockId { get; set; }

        public Guid BandId { get; set; }

        public CurriculumBlock? Block { get; set; }
        public CurriculumBand? Band { get; set; }
    }
}