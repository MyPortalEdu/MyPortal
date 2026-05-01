using MyPortal.Core.Interfaces;
using QueryKit.Repositories.Interfaces;

namespace MyPortal.Data.Interfaces.Repositories.Base;

public interface IEntityRepository<TEntity> : IBaseEntityRepository<TEntity, Guid> where TEntity : class, IEntity
{

}