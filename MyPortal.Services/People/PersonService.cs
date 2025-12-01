using MyPortal.Auth.Interfaces;
using MyPortal.Services.Interfaces.Services;

namespace MyPortal.Services.Services
{
    public class PersonService : BaseService, IPersonService
    {
        public PersonService(IAuthorizationService authorizationService) : base(authorizationService)
        {
        }
    }
}
