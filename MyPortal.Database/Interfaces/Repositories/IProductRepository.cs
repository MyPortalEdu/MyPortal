﻿using MyPortal.Database.Models.Entity;

namespace MyPortal.Database.Interfaces.Repositories
{
    public interface IProductRepository : IReadWriteRepository<Product>, IUpdateRepository<Product>
    {
    }
}