using QueryKit.Repositories.Attributes;

namespace MyPortal.Core.Interfaces;

public interface ISoftDeleteEntity : IEntity
{
    [SoftDelete]
    bool IsDeleted { get; set; }
}