using MyPortal.Logic.Exceptions;
using MyPortal.Logic.Helpers;
using MyPortal.Logic.Interfaces;

namespace MyPortal.Logic.Services
{
    public abstract class BaseService
    {
        protected readonly ISessionUser User;

        protected BaseService(ISessionUser user)
        {
            User = user;
        }

        protected UnauthorisedException Unauthenticated()
        {
            return new UnauthorisedException("The user is not authenticated.");
        }

        protected void Validate<T>(T model)
        {
            ValidationHelper.ValidateModel(model);
        }
    }
}