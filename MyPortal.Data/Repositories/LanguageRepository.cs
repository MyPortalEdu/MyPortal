using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class LanguageRepository : EntityRepository<Language>, ILanguageRepository
{
    public LanguageRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService) : base(
        factory, authorizationService)
    {
    }
}
