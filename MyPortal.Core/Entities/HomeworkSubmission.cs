using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Core.Interfaces;

namespace MyPortal.Core.Entities
{
    [Table("HomeworkSubmissions")]
    public class HomeworkSubmission : Entity, ITaskEntity
    {
        public Guid HomeworkItemId { get; set; }

        public Guid StudentId { get; set; }

        public Guid TaskId { get; set; }

        public Guid? DocumentId { get; set; }

        public int PointsAchieved { get; set; }

        public string? Comments { get; set; }

        public HomeworkItem? HomeworkItem { get; set; }
        public Student? Student { get; set; }
        public Task? Task { get; set; }
        public Document? Document { get; set; }
    }
}