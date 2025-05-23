﻿using MyPortal.Database.Models.Entity;

namespace MyPortal.Database.Interfaces.Repositories
{
    public interface ISessionRepository : IReadWriteRepository<Session>, IUpdateRepository<Session>
    {
    }
}