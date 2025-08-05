using System;
using System.Collections.Generic;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MyPortal.Database.Exceptions;
using MyPortal.Database.Helpers;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Database.Models.Connection;
using MyPortal.Database.Models.Entity;
using MyPortal.Database.Models.Search;
using MyPortal.Database.Repositories.Base;
using SqlKata;

namespace MyPortal.Database.Repositories
{
    public class TaskRepository : BaseReadWriteRepository<Task>, ITaskRepository
    {
        public TaskRepository(DbUserWithContext dbUser) : base(dbUser)
        {
        }
        
        protected override string TableName => "Tasks";

        protected override Query GetDefaultQuery(bool includeSoftDeleted = false)
        {
            var query = new Query($"{TableName} as {TableAlias}");

            query.LeftJoin("HomeworkSubmissions as HS", "T.Id", "HS.TaskId");
            query.LeftJoin("HomeworkItems as HI", "HS.HomeworkId", "HI.Id");

            query.Select($"{TableAlias}.Id");
            query.Select($"{TableAlias}.TypeId");
            query.Select($"{TableAlias}.Id");
            query.Select($"{TableAlias}.AssignedToId");
            query.Select($"{TableAlias}.CreatedById");
            query.Select($"{TableAlias}.CreatedDate");
            query.Select($"{TableAlias}.DueDate");
            query.Select($"{TableAlias}.CompletedDate");
            query.SelectRaw($"COALESCE(HI.Title, {TableAlias}.Title) as [Title]");
            query.SelectRaw($"COALESCE(HI.Description, {TableAlias}.Description) as [Description]");
            query.Select($"{TableAlias}.Completed");
            query.Select($"{TableAlias}.AllowEdit");
            query.Select($"{TableAlias}.System");

            JoinRelated(query);
            SelectAllRelated(query);

            return query;
        }

        protected override Query JoinRelated(Query query)
        {
            query.LeftJoin("People as AT", "AT.Id", $"{TableAlias}.AssignedToId");
            query.LeftJoin("Users as AB", "AB.Id", $"{TableAlias}.CreatedById");
            query.LeftJoin("TaskTypes as TT", "TT.Id", $"{TableAlias}.TypeId");

            return query;
        }

        protected override Query SelectAllRelated(Query query)
        {
            query.SelectAllColumns(typeof(Person), "AT");
            query.SelectAllColumns(typeof(User), "AB");
            query.SelectAllColumns(typeof(TaskType), "TT");

            return query;
        }

        protected override async System.Threading.Tasks.Task<IEnumerable<Task>> ExecuteQuery(Query query)
        {
            var sql = Compiler.Compile(query);

            var tasks = await DbUser.Transaction.Connection.QueryAsync<Task, Person, User, TaskType, Task>(sql.Sql,
                (task, person, user, type) =>
                {
                    task.AssignedTo = person;
                    task.CreatedBy = user;
                    task.Type = type;

                    return task;
                }, sql.NamedBindings, DbUser.Transaction);

            return tasks;
        }

        public async System.Threading.Tasks.Task<IEnumerable<Task>> GetByAssignedTo(Guid personId,
            TaskSearchOptions searchOptions = null)
        {
            var query = GetDefaultQuery();

            query.Where($"{TableAlias}.AssignedToId", personId);

            if (searchOptions != null)
            {
                ApplySearch(query, searchOptions);
            }

            return await ExecuteQuery(query);
        }

        private void ApplySearch(Query query, TaskSearchOptions searchOptions)
        {
            if (searchOptions.Status == TaskStatus.Overdue)
            {
                query.Where($"{TableAlias}.Completed", false);
                query.Where($"{TableAlias}.DueDate", "<", DateTime.Today);
            }

            else if (searchOptions.Status == TaskStatus.Active)
            {
                query.Where($"{TableAlias}.Completed", false);
            }

            else if (searchOptions.Status == TaskStatus.Completed)
            {
                query.Where($"{TableAlias}.Completed", true);
            }
        }

        public async System.Threading.Tasks.Task Update(Task entity)
        {
            var task = await DbUser.Context.Tasks.FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (task == null)
            {
                throw new EntityNotFoundException("Task not found.");
            }

            task.Completed = entity.Completed;
            task.Description = entity.Description;
            task.TypeId = entity.TypeId;
            task.DueDate = entity.DueDate;
            task.CompletedDate = entity.CompletedDate;
        }
    }
}