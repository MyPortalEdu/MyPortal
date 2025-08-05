using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MyPortal.Database.Exceptions;
using MyPortal.Database.Helpers;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Database.Models.Connection;
using MyPortal.Database.Models.Entity;
using MyPortal.Database.Repositories.Base;
using SqlKata;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Database.Repositories
{
    public class DiaryEventRepository : BaseReadWriteRepository<DiaryEvent>, IDiaryEventRepository
    {
        public DiaryEventRepository(DbUserWithContext dbUser) : base(dbUser)
        {
        }
        
        protected override string TableName => "DiaryEvents";

        protected override Query JoinRelated(Query query)
        {
            query.LeftJoin("DiaryEventTypes as DET", "DET.Id", $"{TableAlias}.EventTypeId");
            query.LeftJoin("Rooms as R", "R.Id", $"{TableAlias}.RoomId");
            query.LeftJoin("Users as U", "U.Id", $"{TableAlias}.CreatedById");

            return query;
        }

        private Query JoinEventTypeEntities(Query query)
        {
            query.LeftJoin("Detentions as D", "D.EventId", $"{TableAlias}.Id");
            query.LeftJoin("ParentEvenings as PE", "PE.EventId", $"{TableAlias}.Id");

            return query;
        }

        private Query JoinEventTypePeople(Query query)
        {
            // Detentions
            query.LeftJoin("StudentIncidentDetentions as SID", "D.Id", "SID.DetentionId");
            query.LeftJoin("StudentIncidents as SI", "SID.StudentIncidentId", "SI.Id");
            query.LeftJoin("Students as DetentionStudents", "SI.StudentId", "DetentionStudents.Id");
            query.LeftJoin("StaffMembers as DetentionStaff", "D.SupervisorId", "DetentionStaff.Id");

            // Parent Evenings
            query.LeftJoin("ParentEveningStaffMembers as PESM", "PE.Id", "PESM.ParentEveningId");
            query.LeftJoin("ParentEveningAppointments as PEA", "PESM.Id", "PEA.ParentEveningStaffId");
            query.LeftJoin("Students as ParentEveningStudents", "PEA.StudentId", "ParentEveningStudents.Id");
            query.LeftJoin("StaffMembers as ParentEveningStaff", "PESM.StaffMemberId", "ParentEveningStaff.Id");

            return query;
        }

        protected override Query SelectAllRelated(Query query)
        {
            query.SelectAllColumns(typeof(DiaryEventType), "DET");
            query.SelectAllColumns(typeof(Room), "R");
            query.SelectAllColumns(typeof(User), "U");

            return query;
        }

        protected override async Task<IEnumerable<DiaryEvent>> ExecuteQuery(Query query)
        {
            var sql = Compiler.Compile(query);

            var events =
                await DbUser.Transaction.Connection.QueryAsync<DiaryEvent, DiaryEventType, Room, User, DiaryEvent>(
                    sql.Sql,
                    (diaryEvent, type, room, user) =>
                    {
                        diaryEvent.EventType = type;
                        diaryEvent.Room = room;
                        diaryEvent.CreatedBy = user;

                        return diaryEvent;
                    }, sql.NamedBindings, DbUser.Transaction);

            return events;
        }

        public async Task<IEnumerable<DiaryEvent>> GetByDateRange(DateTime firstDate, DateTime lastDate,
            bool includePrivateEvents = false)
        {
            var query = GetDefaultQuery();

            query.WhereDate($"{TableAlias}.StartTime", ">=", firstDate.Date);
            query.WhereDate($"{TableAlias}.EndTime", "<", lastDate.Date.AddDays(1));

            if (!includePrivateEvents)
            {
                query.WhereTrue($"{TableAlias}.IsPublic");
            }

            return await ExecuteQuery(query);
        }

        public async Task<IEnumerable<DiaryEvent>> GetByPerson(DateTime firstDate, DateTime lastDate, Guid personId)
        {
            var query = GetDefaultQuery();

            query.LeftJoin("DiaryEventAttendees as A", "A.EventId", $"{TableAlias}.Id");

            JoinEventTypeEntities(query);
            JoinEventTypePeople(query);

            query.Where($"{TableAlias}.StartTime", ">=", firstDate.Date);

            // Events might start today but go on for 2 weeks (unlikely but still a use case)
            // we want to include these events but exclude events that start after the end date
            query.Where($"{TableAlias}.StartTime", "<=", lastDate.Date.AddTicks(TimeSpan.TicksPerDay - 1));

            query.Where(q =>
            {
                q.Where("A.PersonId", personId);
                q.OrWhere("DetentionStudents.PersonId", personId);
                q.OrWhere("DetentionStaff.PersonId", personId);
                q.OrWhere("ParentEveningStudents.PersonId", personId);
                q.OrWhere("ParentEveningStaff.PersonId", personId);

                return q;
            });

            return await ExecuteQuery(query);
        }

        public async Task<IEnumerable<DiaryEvent>> GetPublicEvents(DateTime firstDate, DateTime lastDate)
        {
            var query = GetDefaultQuery();

            query.Where($"{TableAlias}.Public", true);

            return await ExecuteQuery(query);
        }

        public async Task<IEnumerable<DiaryEvent>> GetByRoom(DateTime firstDate, DateTime lastDate, Guid roomId)
        {
            var query = GetDefaultQuery();

            query.WhereDate($"{TableAlias}.StartTime", ">=", firstDate.Date);

            // Events might start today but go on for 2 weeks (unlikely but still a use case)
            // we want to include these events but exclude events that start after the end date
            query.WhereDate($"{TableAlias}.StartTime", "<=", lastDate.Date.AddTicks(TimeSpan.TicksPerDay - 1));

            query.Where($"{TableAlias}.RoomId", roomId);

            return await ExecuteQuery(query);
        }

        public async Task Update(DiaryEvent entity)
        {
            var diaryEvent = await DbUser.Context.DiaryEvents.FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (diaryEvent == null)
            {
                throw new EntityNotFoundException("Calendar event not found.");
            }

            diaryEvent.EventTypeId = entity.EventTypeId;
            diaryEvent.RoomId = entity.RoomId;
            diaryEvent.Subject = entity.Subject;
            diaryEvent.Description = entity.Description;
            diaryEvent.Location = entity.Location;
            diaryEvent.StartTime = entity.StartTime;
            diaryEvent.EndTime = entity.EndTime;
            diaryEvent.Public = entity.Public;
            diaryEvent.AllDay = entity.AllDay;
        }
    }
}