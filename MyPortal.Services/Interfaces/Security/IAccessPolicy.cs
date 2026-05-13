namespace MyPortal.Services.Interfaces.Security
{
    public interface IAccessPolicy<in TEntity, in TScope>
    {
        Task<bool> CanViewAsync(TEntity entity, TScope scope, CancellationToken cancellationToken);
        bool CanEdit(TEntity entity, TScope scope);
    }
}
