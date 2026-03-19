namespace MyPortal.Core.Interfaces;

public interface ILockableEntity : IEntity
{
    public Guid? LockedById { get; set; }
    public DateTime? LockedAt { get; set; }
    public Guid? LockToken { get; set; }
}