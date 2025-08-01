using System;
using System.Threading.Tasks;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Logic.Helpers;
using MyPortal.Logic.Interfaces;
using MyPortal.Logic.Interfaces.Services;

namespace MyPortal.Logic.Services
{
    public sealed class SchoolService : BaseService, ISchoolService
    {
        private readonly IUserService _userService;
        
        public SchoolService(ISessionUser user, IUserService userService) : base(user)
        {
            _userService = userService;
        }
        
        private async Task<string> GetLocalSchoolNameFromDb()
        {
            await using var unitOfWork = await User.GetConnection();

            var localSchoolName = await unitOfWork.GetRepository<ISchoolRepository>().GetLocalSchoolName();

            return localSchoolName;
        }

        public async Task<string> GetLocalSchoolName()
        {
            var localSchoolName =
                await CacheHelper.StringCache.GetOrCreate(CacheKeys.LocalSchoolName, GetLocalSchoolNameFromDb,
                    TimeSpan.FromHours(24));

            return localSchoolName;
        }
    }
}