namespace MyPortal.Services.Interfaces.Security
{
    public interface IAccessPolicy<in TEntity, in TScope>
    {
        bool CanView(TEntity entity, TScope scope);
        bool CanEdit(TEntity entity, TScope scope);
    }
}
