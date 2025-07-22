using System;
using System.Threading.Tasks;
using MyPortal.Database.Interfaces.Repositories;

namespace MyPortal.Database.Interfaces
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        int BatchLimit { get; set; }
        Task BatchSaveChangesAsync();
        Task SaveChangesAsync();
        Task<bool> GetLock(string name, int timeout = 0);
    }
}