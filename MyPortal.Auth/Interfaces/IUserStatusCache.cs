namespace MyPortal.Auth.Interfaces;

public interface IUserStatusCache
{
    Task<bool> IsEnabledAsync(
        Guid userId,
        Func<CancellationToken, Task<bool>> factory,
        CancellationToken ct = default);

    void Invalidate(Guid userId);
}
