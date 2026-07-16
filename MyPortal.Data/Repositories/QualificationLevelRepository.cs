using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class QualificationLevelRepository : EntityRepository<QualificationLevel>, IQualificationLevelRepository
{
    public QualificationLevelRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) :
        base(factory, authorizationService)
    {
    }
}
