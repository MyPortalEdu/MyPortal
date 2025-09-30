using MyPortal.Core.Interfaces;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Services.Interfaces.Repositories.Base;

public interface IEntityReadRepository<TEntity> : IBaseEntityReadRepository<TEntity, Guid>
    where TEntity : class, IEntity
{
    
}