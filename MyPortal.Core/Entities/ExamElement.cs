using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamElements")]
    public class ExamElement : Entity
    {
        public Guid BaseElementId { get; set; }

        public Guid SeriesId { get; set; }
        
        [StringLength(256)]
        public string? Description { get; set; }
        
        public decimal? ExamFee { get; set; }

        public bool IsSubmitted { get; set; }

        public ExamBaseElement? BaseElement { get; set; }
        public ExamSeries? Series { get; set; }
    }
}