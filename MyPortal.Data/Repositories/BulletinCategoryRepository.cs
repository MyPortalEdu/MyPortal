using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class BulletinCategoryRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<BulletinCategory>(factory, authorizationService), IBulletinCategoryRepository;
