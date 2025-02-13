﻿using MyPortal.Database.Models.Entity;

namespace MyPortal.Database.Interfaces.Repositories
{
    public interface ISenEventRepository : IReadWriteRepository<SenEvent>, IUpdateRepository<SenEvent>
    {
    }
}