﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MyPortal.Database.Helpers;
using MyPortal.Database.Interfaces;
using MyPortal.Database.Models;

namespace MyPortal.Database.Repositories
{
    public class AttendanceCodeMeaningRepository : BaseReadRepository<AttendanceCodeMeaning>, IAttendanceCodeMeaningRepository
    {
        public AttendanceCodeMeaningRepository(IDbConnection connection) : base(connection)
        {
        }

        protected override async Task<IEnumerable<AttendanceCodeMeaning>> ExecuteQuery(string sql, object param = null)
        {
            return await Connection.QueryAsync<AttendanceCodeMeaning>(sql, param);
        }
    }
}