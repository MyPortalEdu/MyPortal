using Task = MyPortal.Core.Entities.Task;

namespace MyPortal.Core.Interfaces;

public interface ITaskEntity
{
    Guid TaskId { get; set; }
    Task? Task { get; set; }
}