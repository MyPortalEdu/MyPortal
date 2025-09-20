using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("StudentAchievements")]
public class StudentAchievement : Entity
{
    public Guid StudentId { get; set; }

    public Guid AchievementId { get; set; }

    public Guid? OutcomeId { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Achievement cannot have negative points.")]
    public int Points { get; set; }

    public Student? Student { get; set; }
    public Achievement? Achievement { get; set; }
    public AchievementOutcome? Outcome { get; set; }
}