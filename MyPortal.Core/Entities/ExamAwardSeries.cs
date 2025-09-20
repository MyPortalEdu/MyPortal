using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamAwardSeries")]
    public class ExamAwardSeries : Entity
    {
        public Guid AwardId { get; set; }

        public Guid SeriesId { get; set; }

        public ExamAward? Award { get; set; }
        public ExamSeries? Series { get; set; }
    }
}