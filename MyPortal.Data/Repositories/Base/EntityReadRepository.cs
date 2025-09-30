using MyPortal.Core.Interfaces;
using MyPortal.Services.Interfaces.Repositories.Base;
using QueryKit.Repositories;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Repositories.Base;

public class EntityReadRepository<TEntity> : BaseEntityReadRepository<TEntity, Guid>, IEntityReadRepository<TEntity>
    where TEntity : class, IEntity
{
    protected EntityReadRepository(IConnectionFactory factory) : base(factory)
    {
    }
}