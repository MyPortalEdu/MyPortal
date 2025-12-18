using System.Security.Authentication;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Interfaces;
using MyPortal.Services.Interfaces.Repositories.Base;
using QueryKit.Repositories;

namespace MyPortal.Data.Repositories.Base;

public class EntityRepository<TEntity> : BaseEntityRepository<TEntity, Guid>, IEntityRepository<TEntity>
    where TEntity : class, IEntity
{
    private readonly IAuthorizationService _authorizationService;
    
    public EntityRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) : base(factory)
    {
        _authorizationService = authorizationService;
    }

    public override Task<TEntity> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is IAuditableEntity auditable)
        {
            var userId = _authorizationService.GetCurrentUserId();

            if (!userId.HasValue)
            {
                throw new AuthenticationException("Not authenticated.");
            }
            
            var ipAddress = _authorizationService.GetCurrentUserIpAddress() ?? "";
            
            auditable.CreatedAt = DateTime.UtcNow;
            auditable.CreatedById = userId.Value;
            auditable.CreatedByIpAddress = ipAddress;
            auditable.LastModifiedByIpAddress = ipAddress;
            auditable.LastModifiedAt = DateTime.UtcNow;
            auditable.LastModifiedById = userId.Value;
        }
        
        return base.InsertAsync(entity, cancellationToken);
    }

    public override async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var entityInDb = await GetByIdAsync(entity.Id, cancellationToken);

        if (entityInDb is ISystemEntity { IsSystem: true })
        {
            throw new SystemEntityException("You cannot modify a system entity.");
        }

        if (entity is IAuditableEntity auditable)
        {
            var userId = _authorizationService.GetCurrentUserId();

            if (!userId.HasValue)
            {
                throw new AuthenticationException("Not authenticated.");
            }
            
            var ipAddress = _authorizationService.GetCurrentUserIpAddress() ?? "";
            
            auditable.LastModifiedById = userId.Value;
            auditable.LastModifiedAt = DateTime.UtcNow;
            auditable.LastModifiedByIpAddress = ipAddress;
        }

        return await base.UpdateAsync(entity, cancellationToken);
    }

    public override async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default, bool softDelete = true)
    {
        var entity = await GetByIdAsync(id, cancellationToken);

        if (entity is ISystemEntity { IsSystem: true })
        {
            throw new SystemEntityException("You cannot delete a system entity.");
        }

        return await base.DeleteAsync(id, cancellationToken, softDelete);
    }
}