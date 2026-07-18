using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

/// <inheritdoc cref="IPhotoRepository"/>
public class PhotoRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<Photo>(factory, authorizationService), IPhotoRepository;
