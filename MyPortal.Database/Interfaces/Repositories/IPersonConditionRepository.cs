﻿using MyPortal.Database.Models.Entity;

namespace MyPortal.Database.Interfaces.Repositories
{
    public interface IPersonConditionRepository : IReadWriteRepository<PersonCondition>,
        IUpdateRepository<PersonCondition>
    {
    }
}