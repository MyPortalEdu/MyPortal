using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MyPortal.Database.Helpers;
using MyPortal.Database.Interfaces;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Database.Models;
using MyPortal.Database.Models.Connection;

namespace MyPortal.Database
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _context;
        private int _batchSize;
        private int _batchLimit = 1000;
        private readonly string _connectionString;
        private DbTransaction _transaction;
        private readonly Guid _userId;
        private readonly Dictionary<Type, IRepository> _repositories = new Dictionary<Type, IRepository>();

        public static async Task<IUnitOfWork> Create(Guid userId, ApplicationDbContext context)
        {
            var unitOfWork = new UnitOfWork(userId, context);
            await unitOfWork.Initialise();
            return unitOfWork;
        }

        public TRepository GetRepository<TRepository>() where TRepository : IRepository
        {
            if (_repositories.ContainsKey(typeof(TRepository)))
            {
                return  (TRepository)_repositories[typeof(TRepository)];
            }
            
            var repository = DbFactory.CreateRepository<TRepository>(GetDbUserWithContext());
            _repositories.Add(typeof(TRepository), repository);
            return repository;
        }

        private DbUser GetDbUser()
        {
            return new DbUser(_userId, _transaction);
        }

        private DbUserWithContext GetDbUserWithContext()
        {
            return new DbUserWithContext(_userId, _transaction, _context);
        }

        private async Task<DbTransaction> GetDbTransaction(bool useContext)
        {
            if (useContext)
            {
                // Use this to utilise the context's own transaction
                var contextTransaction = await _context.Database.BeginTransactionAsync();
                var transaction = contextTransaction.GetDbTransaction();
                return transaction;
            }

            // Use this to create a transaction separate from the context
            if (!string.IsNullOrWhiteSpace(_connectionString))
            {
                var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var transaction = await connection.BeginTransactionAsync();
                return transaction;
            }

            return null;
        }

        private async Task Initialise()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
                ResetRepositories();
            }

            _transaction = await GetDbTransaction(false);
        }

        private UnitOfWork(Guid userId, ApplicationDbContext context)
        {
            _userId = userId;
            _context = context;
            _connectionString = context.Database.GetConnectionString();
        }

        public int BatchLimit
        {
            get => _batchLimit;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("Batch limit cannot be less than 0.");
                }

                _batchLimit = value;
            }
        }

        public void Dispose()
        {
            var connection = _transaction.Connection;
            _transaction?.Dispose();
            connection?.Dispose();
            _context?.Dispose();
        }

        public async Task BatchSaveChangesAsync()
        {
            _batchSize++;

            if (_batchSize >= _batchLimit)
            {
                await SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();

                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }

                _batchSize = 0;
            }
            catch (Exception)
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                }

                throw;
            }
            finally
            {
                await Initialise();
            }
        }

        public async Task<bool> GetLock(string name, int timeout = 0)
        {
            if (_transaction != null)
            {
                return await DatabaseHelper.TryGetApplicationLock(_transaction, name, timeout);
            }

            return false;
        }

        private void ResetRepositories()
        {
            _repositories.Clear();
        }

        public async ValueTask DisposeAsync()
        {
            ResetRepositories();

            var connection = _transaction.Connection;

            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }

            if (connection != null)
            {
                await connection.DisposeAsync();
            }

            if (_context != null)
            {
                await _context.DisposeAsync();
            }
        }
    }
}