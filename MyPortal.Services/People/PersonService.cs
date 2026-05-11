using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Core.Entities;
using MyPortal.Services.Interfaces.People;

namespace MyPortal.Services.People
{
    public class PersonService : BaseService, IPersonService
    {
        public PersonService(IAuthorizationService authorizationService, ILogger<PersonService> logger) : base(authorizationService,
            logger)
        {
        }
    }
}
