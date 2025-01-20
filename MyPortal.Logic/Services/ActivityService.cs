using MyPortal.Logic.Interfaces;
using MyPortal.Logic.Interfaces.Services;

namespace MyPortal.Logic.Services
{
    public sealed class ActivityService : BaseService, IActivityService
    {
        public ActivityService(ISessionUser user) : base(user)
        {
        }
    }
}