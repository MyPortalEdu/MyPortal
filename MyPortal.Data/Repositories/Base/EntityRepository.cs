using MyPortal.Common.Interfaces;
using MyPortal.Core.Interfaces;
using MyPortal.Services.Interfaces.Repositories.Base;
using QueryKit.Repositories;

namespace MyPortal.Data.Repositories.Base;

public class EntityRepository<TEntity> : BaseEntityRepository<TEntity, Guid>, IEntityRepository<TEntity>
    where TEntity : class, IEntity
{
    protected EntityRepository(IDbConnectionFactory factory) : base(factory)
    {
    }
}