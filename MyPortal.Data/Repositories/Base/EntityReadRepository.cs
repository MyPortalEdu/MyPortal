using MyPortal.Common.Interfaces;
using MyPortal.Core.Interfaces;
using MyPortal.Services.Interfaces.Repositories.Base;
using QueryKit.Repositories;

namespace MyPortal.Data.Repositories.Base;

public class EntityReadRepository<TEntity> : BaseEntityReadRepository<TEntity, Guid>, IEntityReadRepository<TEntity>
    where TEntity : class, IEntity
{
    protected EntityReadRepository(IDbConnectionFactory factory) : base(factory)
    {
    }
}