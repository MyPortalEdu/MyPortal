namespace MyPortal.Auth.Interfaces;

public interface IAuthorizationService
{
    Task RequireAsync(string permission, CancellationToken ct = default);
}