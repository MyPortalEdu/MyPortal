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
        public string SeriesCode { get; set; } = null!;

        [Required]
        public string Code { get; set; } = null!;

        [Required]
        public string Title { get; set; } = null!;

        public ExamSeason? Season { get; set; }
        public ExamBoard? ExamBoard { get; set; }
    }
}