using MyPortal.Common.Exceptions;
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

    public override async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entityInDb = await GetByIdAsync(entity.Id, cancellationToken);

        if (entityInDb is ISystemEntity { IsSystem: true })
        {
            throw new SystemEntityException("You cannot modify a system entity.");
        }

        return await base.UpdateAsync(entity, cancellationToken);
    }

    public override async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);

        if (entity is ISystemEntity { IsSystem: true })
        {
            throw new SystemEntityException("You cannot delete a system entity.");
        }

        return await base.DeleteAsync(id, cancellationToken);
    }
}