﻿using System.Collections.Generic;
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
    public class ReportCardRepository : BaseReadWriteRepository<ReportCard>, IReportCardRepository
    {
        public ReportCardRepository(DbUserWithContext dbUser) : base(dbUser)
        {
        }

        protected override Query JoinRelated(Query query)
        {
            query.LeftJoin("Students as S", "S.Id", $"{TblAlias}.StudentId");
            query.LeftJoin("IncidentType as IT", "IT.Id", $"{TblAlias}.BehaviourTypeId");

            return query;
        }

        protected override Query SelectAllRelated(Query query)
        {
            query.SelectAllColumns(typeof(Student), "S");
            query.SelectAllColumns(typeof(IncidentType), "IT");

            return query;
        }

        protected override async Task<IEnumerable<ReportCard>> ExecuteQuery(Query query)
        {
            var sql = Compiler.Compile(query);

            var reportCards =
                await DbUser.Transaction.Connection.QueryAsync<ReportCard, Student, IncidentType, ReportCard>(
                    sql.Sql,
                    (card, student, type) =>
                    {
                        card.Student = student;
                        card.BehaviourType = type;

                        return card;
                    }, sql.NamedBindings, DbUser.Transaction);

            return reportCards;
        }

        public async Task Update(ReportCard entity)
        {
            var reportCard = await DbUser.Context.ReportCards.FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (reportCard == null)
            {
                throw new EntityNotFoundException("Report card not found.");
            }

            reportCard.StartDate = entity.StartDate;
            reportCard.EndDate = entity.EndDate;
            reportCard.Comments = entity.Comments;
            reportCard.Active = entity.Active;
        }
    }
}