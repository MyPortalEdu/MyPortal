using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities;

[Table("UserReminderSettings")]
public class UserReminderSetting : Entity
{
    public Guid UserId { get; set; }

    public Guid ReminderType { get; set; }

    public TimeSpan RemindBefore { get; set; }

    public User? User { get; set; }
}