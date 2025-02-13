﻿using System;
using System.Threading.Tasks;
using MyPortal.Database.Models.Entity;

namespace MyPortal.Database.Interfaces.Repositories
{
    public interface IStaffMemberRepository : IReadWriteRepository<StaffMember>, IUpdateRepository<StaffMember>
    {
        Task<StaffMember> GetByPersonId(Guid personId);

        Task<StaffMember> GetByUserId(Guid userId);
    }
}