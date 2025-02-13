﻿using System;
using System.Threading.Tasks;
using MyPortal.Database.Models.Entity;

namespace MyPortal.Database.Interfaces.Repositories
{
    public interface IFileRepository : IReadWriteRepository<File>, IUpdateRepository<File>
    {
        Task<File> GetByDocumentId(Guid documentId);
    }
}