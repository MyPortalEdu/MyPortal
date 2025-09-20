using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("ExamResultEmbargoes")]
    public class ExamResultEmbargo : Entity
    {
        public Guid ResultSetId { get; set; }

        public DateTime EndTime { get; set; }

        public ResultSet? ResultSet { get; set; }
    }
}