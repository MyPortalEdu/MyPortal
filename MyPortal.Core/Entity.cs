using MyPortal.Core.Interfaces;

namespace MyPortal.Core;

public abstract class Entity : IEntity
{
    public Guid Id { get; set; }
}