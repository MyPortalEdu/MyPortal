﻿using MyPortal.Database.Models.Entity;

namespace MyPortal.Database.Interfaces.Repositories
{
    public interface ISubjectStaffMemberRepository : IReadWriteRepository<SubjectStaffMember>,
        IUpdateRepository<SubjectStaffMember>
    {
    }
}