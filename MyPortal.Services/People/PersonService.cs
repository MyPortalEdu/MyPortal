using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Services.Interfaces.Services;

namespace MyPortal.Services.People
{
    public class PersonService : BaseService, IPersonService
    {
        public PersonService(IAuthorizationService authorizationService, ILogger logger) : base(authorizationService,
            logger)
        {
        }
    }
}
