using QueryKit.Attributes;

namespace MyPortal.Core.Interfaces;

public interface IVersionedEntity : IEntity
{
    [Version]
    long Version { get; set;  }
}