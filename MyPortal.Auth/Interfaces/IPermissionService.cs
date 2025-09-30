namespace MyPortal.Auth.Interfaces;

public interface IPermissionService
{
    Task<bool> HasAsync(Guid userId, string permission, CancellationToken ct = default);
}