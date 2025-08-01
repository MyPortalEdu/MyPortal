using System;
using System.Threading.Tasks;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Logic.Exceptions;
using MyPortal.Logic.Interfaces;
using MyPortal.Logic.Interfaces.Services;

namespace MyPortal.Logic.Services
{
    public sealed class SystemSettingService : BaseService, ISystemSettingService
    {
        public SystemSettingService(ISessionUser user) : base(user)
        {
        }

        public async Task SetValue(string name, string value)
        {
            await using var unitOfWork = await User.GetConnection();

            await unitOfWork.GetRepository<ISystemSettingRepository>().Update(name, value);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GetDatabaseVersion()
        {
            await using var unitOfWork = await User.GetConnection();

            var databaseVersion = await unitOfWork.GetRepository<ISystemSettingRepository>().Get("DatabaseVersion");

            if (databaseVersion == null)
            {
                throw new NotFoundException("Database version not found.");
            }

            return Convert.ToInt32(databaseVersion.Setting);
        }
    }
}