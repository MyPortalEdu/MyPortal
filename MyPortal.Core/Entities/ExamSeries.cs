using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamSeries")]
    public class ExamSeries : Entity
    {
        public Guid ExamBoardId { get; set; }

        public Guid ExamSeasonId { get; set; }

        [Required]
        public required string SeriesCode { get; set; }

        [Required]
        public required string Code { get; set; }

        [Required]
        public required string Title { get; set; }

        public ExamSeason? Season { get; set; }
        public ExamBoard? ExamBoard { get; set; }
    }
}