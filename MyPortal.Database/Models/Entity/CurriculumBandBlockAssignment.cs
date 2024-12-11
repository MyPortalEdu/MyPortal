using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Database.Models.Entity
{
    [Table("curriculum_band_block_assignment")]
    public class CurriculumBandBlockAssignment : BaseTypes.Entity
    {
        [Column(Order = 2)] public Guid BlockId { get; set; }

        [Column(Order = 3)] public Guid BandId { get; set; }

        public virtual CurriculumBlock Block { get; set; }
        public virtual CurriculumBand Band { get; set; }
    }
}