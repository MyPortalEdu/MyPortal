using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("TaskReminders")]
public class TaskReminder : Entity
{
    public Guid TaskId { get; set; }

    public Guid UserId { get; set; }

    public DateTime RemindTime { get; set; }

    public Task? Task { get; set; }
}