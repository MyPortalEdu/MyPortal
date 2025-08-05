using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MyPortal.Database.Helpers;
using MyPortal.Database.Interfaces;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Database.Models.Connection;
using SqlKata;

namespace MyPortal.Database.Repositories.Base
{
    public abstract class BaseReadRepository<TEntity> : BaseRepository, IReadRepository<TEntity>
        where TEntity : class, IEntity
    {
        public BaseReadRepository(DbUser dbUser, string tableAlias = null) : base(dbUser)
        {
            TableAlias = tableAlias;
        }

        protected abstract string TableName { get; }

        protected string TableAlias;

        protected string TableReference =>
            !string.IsNullOrWhiteSpace(TableAlias) ? $"[{TableName}] AS [{TableAlias}]" : TableName;

        protected virtual Query JoinRelated(Query query)
        {
            return query;
        }

        protected virtual Query SelectAllRelated(Query query)
        {
            return query;
        }

        protected virtual async Task<IEnumerable<TEntity>> ExecuteQuery(Query query)
        {
            return await ExecuteQuery<TEntity>(query);
        }

        protected async Task<IEnumerable<T>> ExecuteQuery<T>(Query query)
        {
            var sql = Compiler.Compile(query);

            return await DbUser.Transaction.Connection.QueryAsync<T>(sql.Sql, sql.NamedBindings, DbUser.Transaction);
        }

        protected async Task<TEntity> ExecuteQueryFirstOrDefault(Query query)
        {
            var result = await ExecuteQuery(query);

            return result.FirstOrDefault();
        }

        protected virtual Query GetDefaultQuery(bool includeSoftDeleted = false)
        {
            var query = new Query($"{TableReference}").SelectAllColumns(typeof(TEntity), TableAlias);

            JoinRelated(query);
            SelectAllRelated(query);

            if (typeof(TEntity).GetInterfaces().Contains(typeof(ISoftDeleteEntity)) && !includeSoftDeleted)
            {
                query.Where($"{TableAlias}.Deleted", false);
            }

            return query;
        }

        protected Query GetDefaultQuery(Type t, bool includeSoftDeleted = false)
        {
            var tableIdentifier = !string.IsNullOrWhiteSpace(TableAlias) ? TableAlias : TableName;
            
            var query = new Query(TableReference).SelectAllColumns(t, tableIdentifier);

            if (t.GetInterfaces().Contains(typeof(ISoftDeleteEntity)) && !includeSoftDeleted)
            {
                query.Where($"{tableIdentifier}.Deleted", false);
            }

            return query;
        }

        public async Task<IEnumerable<TEntity>> GetAll()
        {
            var sql = GetDefaultQuery();

            return await ExecuteQuery(sql);
        }

        public async Task<TEntity> GetById(Guid id)
        {
            var query = GetDefaultQuery();

            query.Where($"{TableAlias}.Id", id);

            return (await ExecuteQuery(query)).SingleOrDefault();
        }

        protected async Task<T> ExecuteQueryFirstOrDefault<T>(Query query)
        {
            var sql = Compiler.Compile(query);

            return await DbUser.Transaction.Connection.QueryFirstOrDefaultAsync<T>(sql.Sql, sql.NamedBindings,
                DbUser.Transaction);
        }

        protected Query GetEmptyQuery()
        {
            return GetEmptyQuery(TableName, TableAlias);
        }

        protected Query GetEmptyQuery(string tableName, string tableAlias)
        {
            var tableIdentifier = !string.IsNullOrWhiteSpace(tableAlias)
                ? $"[{tableName}] as [{tableAlias}]"
                : $"[{tableName}]";
            return new Query(tableIdentifier);
        }

        protected async Task<int?> ExecuteQueryIntResult(Query query)
        {
            var sql = Compiler.Compile(query);

            var result =
                await DbUser.Transaction.Connection.QueryFirstOrDefaultAsync<int?>(sql.Sql, sql.NamedBindings,
                    DbUser.Transaction);
            return result;
        }

        protected async Task<string> ExecuteQueryStringResult(Query query)
        {
            var sql = Compiler.Compile(query);

            return await DbUser.Transaction.Connection.QuerySingleOrDefaultAsync<string>(sql.Sql, sql.NamedBindings,
                DbUser.Transaction);
        }
    }
}