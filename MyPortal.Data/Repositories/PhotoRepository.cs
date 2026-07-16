using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

/// <inheritdoc cref="IPhotoRepository"/>
public class PhotoRepository : EntityRepository<Photo>, IPhotoRepository
{
    public PhotoRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
        : base(factory, authorizationService)
    {
    }
}
