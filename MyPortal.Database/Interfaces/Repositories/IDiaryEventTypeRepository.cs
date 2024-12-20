﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MyPortal.Database.Models.Entity;

namespace MyPortal.Database.Interfaces.Repositories
{
    public interface IDiaryEventTypeRepository : IReadWriteRepository<DiaryEventType>, IUpdateRepository<DiaryEventType>
    {
        Task<IEnumerable<DiaryEventType>> GetAll(bool includeReserved);
    }
}