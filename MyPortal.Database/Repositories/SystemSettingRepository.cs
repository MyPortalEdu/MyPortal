﻿using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MyPortal.Database.Exceptions;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Database.Models.Connection;
using MyPortal.Database.Models.Entity;
using SqlKata;
using SqlKata.Compilers;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Database.Repositories
{
    public class SystemSettingRepository : ISystemSettingRepository
    {
        private readonly DbUserWithContext _dbUser;

        public SystemSettingRepository(DbUserWithContext dbUser)
        {
            _dbUser = dbUser;
        }

        public async Task<SystemSetting> Get(string name)
        {
            var query = new Query("SystemSettings as SS");

            query.Select("SS.Name, SS.Setting");

            query.Where("SS.Name", name);

            var sql = new SqlServerCompiler().Compile(query);

            return (await _dbUser.Transaction.Connection.QueryAsync<SystemSetting>(sql.Sql, sql.NamedBindings,
                _dbUser.Transaction)).FirstOrDefault();
        }

        public async Task Update(string name, string value)
        {
            var setting = await _dbUser.Context.SystemSettings.FirstOrDefaultAsync(x => x.Name == name);

            if (setting == null)
            {
                throw new EntityNotFoundException($"System setting '{name}' not found.");
            }

            setting.Setting = value;
        }
    }
}