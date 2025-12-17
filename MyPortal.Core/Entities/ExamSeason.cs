using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamSeasons")]
    public class ExamSeason : Entity
    {
        public Guid ResultSetId { get; set; }

        public int CalendarYear { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Required] 
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsDefault { get; set; }

        public ResultSet? ResultSet { get; set; }
    }
}