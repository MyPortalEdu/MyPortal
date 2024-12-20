﻿using MyPortal.Database.Models.Entity;

namespace MyPortal.Database.Interfaces.Repositories
{
    public interface IDetentionTypeRepository : IReadWriteRepository<DetentionType>, IUpdateRepository<DetentionType>
    {
    }
}