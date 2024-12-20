﻿using MyPortal.Database.Models.Entity;

namespace MyPortal.Database.Interfaces.Repositories
{
    public interface IProductTypeRepository : IReadWriteRepository<ProductType>, IUpdateRepository<ProductType>
    {
    }
}