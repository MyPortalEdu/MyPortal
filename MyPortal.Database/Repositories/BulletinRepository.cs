using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MyPortal.Database.Enums;
using MyPortal.Database.Exceptions;
using MyPortal.Database.Helpers;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Database.Models.Connection;
using MyPortal.Database.Models.Entity;
using MyPortal.Database.Models.Filters;
using MyPortal.Database.Models.Paging;
using MyPortal.Database.Models.QueryResults.School;
using MyPortal.Database.Models.Search;
using MyPortal.Database.Repositories.Base;
using SqlKata;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Database.Repositories
{
    public class BulletinRepository : BaseReadWriteRepository<Bulletin>, IBulletinRepository
    {
        public BulletinRepository(DbUserWithContext dbUser) : base(dbUser)
        {
        }
        
        protected override string TableName => "Bulletins";

        private void ApplySearch(Query query, BulletinSearchOptions searchOptions)
        {
            if (!string.IsNullOrWhiteSpace(searchOptions.SearchText))
            {
                query.Where(q =>
                    q.WhereContainsWord($"{TableAlias}.Title", searchOptions.SearchText)
                        .OrWhereContainsWord($"{TableAlias}.Summary", searchOptions.SearchText));
            }

            if (!searchOptions.IncludeStaffOnly)
            {
                query.Where($"{TableAlias}.StaffOnly", false);
            }

            if (!searchOptions.IncludeExpired)
            {
                query.Where(q =>
                    q.WhereNull($"{TableAlias}.ExpireDate").OrWhere($"{TableAlias}.ExpireDate", ">", DateTime.Now));
            }

            if (!searchOptions.IncludeUnapproved)
            {
                query.Where($"{TableAlias}.Approved", true);
            }

            if (searchOptions.IncludeCreatedBy.HasValue)
            {
                query.OrWhere($"{TableAlias}.CreatedById", searchOptions.IncludeCreatedBy.Value);
            }
        }

        protected override Query JoinRelated(Query query)
        {
            query.LeftJoin("Users as U", "U.Id", $"{TableAlias}.AuthorId");
            query.LeftJoin("Directories as D", "D.Id", $"{TableAlias}.DirectoryId");

            return query;
        }

        protected override Query SelectAllRelated(Query query)
        {
            query.SelectAllColumns(typeof(User), "U");
            query.SelectAllColumns(typeof(Directory), "D");

            return query;
        }

        protected override async Task<IEnumerable<Bulletin>> ExecuteQuery(Query query)
        {
            var sql = Compiler.Compile(query);

            var bulletins = await DbUser.Transaction.Connection.QueryAsync<Bulletin, User, Directory, Bulletin>(sql.Sql,
                (bulletin, user, dir) =>
                {
                    bulletin.CreatedBy = user;
                    bulletin.Directory = dir;

                    return bulletin;
                }, sql.NamedBindings, DbUser.Transaction);

            return bulletins;
        }

        public async Task<BulletinMetadataPageResponse> GetBulletinDetails(BulletinSearchOptions searchOptions,
            PageFilter pageFilter)
        {
            var query = new Query();

            query.Select($"{TableAlias}.Id");
            query.Select($"{TableAlias}.DirectoryId");
            query.Select($"{TableAlias}.CreatedById");
            query.SelectRaw("COALESCE(D.Name, U.UserName) as CreatedByName");
            query.Select($"{TableAlias}.CreatedDate");
            query.Select($"{TableAlias}.ExpireDate");
            query.Select($"{TableAlias}.Title");
            query.Select($"{TableAlias}.Detail");
            query.Select($"{TableAlias}.Private");
            query.Select($"{TableAlias}.Approved");

            query.FromRaw($@"Bulletins as {TableAlias}");
            query.LeftJoin("Users as U", "U.Id", $"{TableAlias}.CreatedById");
            query.ApplyName("D", "U.PersonId", NameFormat.FullNameAbbreviated);

            query.OrderByDesc($"{TableAlias}.CreatedDate");

            ApplySearch(query, searchOptions);

            if (pageFilter != null)
            {
                query.ApplyPaging(pageFilter);
            }

            var data = await ExecuteQuery<BulletinDetailModel>(query);

            var countQuery = GetEmptyQuery();

            ApplySearch(countQuery, searchOptions);

            var count = await ExecuteQueryIntResult(countQuery.AsCount());

            var response = new BulletinMetadataPageResponse(data, count ?? 0);

            return response;
        }

        public async Task<IEnumerable<BulletinDetailModel>> GetBulletinDetails(BulletinSearchOptions searchOptions)
        {
            var query = new Query();

            query.Select($"{TableAlias}.Id");
            query.Select($"{TableAlias}.DirectoryId");
            query.Select($"{TableAlias}.CreatedById");
            query.Select("D.DisplayName as CreatedByName");
            query.Select($"{TableAlias}.CreatedDate");
            query.Select($"{TableAlias}.ExpireDate");
            query.Select($"{TableAlias}.Title");
            query.Select($"{TableAlias}.Detail");
            query.Select($"{TableAlias}.Private");
            query.Select($"{TableAlias}.Approved");

            query.FromRaw($@"Bulletins as {TableAlias}
CROSS APPLY GetDisplayName({TableAlias}.CreatedById, 2, 1, 1) D");

            query.OrderByDesc($"{TableAlias}.CreatedDate");

            ApplySearch(query, searchOptions);

            var metadata = await ExecuteQuery<BulletinDetailModel>(query);

            return metadata;
        }

        public async Task<IEnumerable<Bulletin>> GetBulletins(BulletinSearchOptions searchOptions)
        {
            var query = GetDefaultQuery();

            ApplySearch(query, searchOptions);

            return await ExecuteQuery(query);
        }

        public async Task<IEnumerable<Bulletin>> GetOwn(Guid authorId)
        {
            var query = GetDefaultQuery();

            query.Where($"{TableAlias}.AuthorId", "=", authorId);

            return await ExecuteQuery(query);
        }

        public async Task Update(Bulletin entity)
        {
            var bulletin = await DbUser.Context.Bulletins.FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (bulletin == null)
            {
                throw new EntityNotFoundException("Bulletin not found.");
            }

            bulletin.Title = entity.Title;
            bulletin.Detail = entity.Detail;
            bulletin.ExpireDate = entity.ExpireDate;
            bulletin.Approved = entity.Approved;
            bulletin.Private = entity.Private;
        }
    }
}