﻿using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using MyPortal.Database.Exceptions;
using MyPortal.Database.Helpers;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Database.Models;
using MyPortal.Database.Models.Entity;
using MyPortal.Database.Repositories.Base;
using SqlKata;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Database.Repositories
{
    public class ExamEnrolmentRepository : BaseReadWriteRepository<ExamEnrolment>, IExamEnrolmentRepository
    {
        public ExamEnrolmentRepository(ApplicationDbContext context, DbTransaction transaction) : base(context, transaction)
        {
        }

        protected override Query JoinRelated(Query query)
        {
            JoinEntity(query, "ExamAwards", "EA", "AwardId");
            JoinEntity(query, "ExamCandidates", "EC", "CandidateId");

            return query;
        }

        protected override Query SelectAllRelated(Query query)
        {
            query.SelectAllColumns(typeof(ExamAward), "EA");
            query.SelectAllColumns(typeof(ExamCandidate), "EC");

            return query;
        }

        protected override async Task<IEnumerable<ExamEnrolment>> ExecuteQuery(Query query)
        {
            var sql = Compiler.Compile(query);

            var enrolments =
                await Transaction.Connection.QueryAsync<ExamEnrolment, ExamAward, ExamCandidate, ExamEnrolment>(sql.Sql,
                    (enrolment, award, candidate) =>
                    {
                        enrolment.Award = award;
                        enrolment.Candidate = candidate;

                        return enrolment;
                    }, sql.NamedBindings, Transaction);

            return enrolments;
        }

        public async Task Update(ExamEnrolment entity)
        {
            var enrolment = await Context.ExamEnrolments.FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (enrolment == null)
            {
                throw new EntityNotFoundException("Enrolment not found.");
            }

            enrolment.StartDate = entity.StartDate;
            enrolment.EndDate = entity.EndDate;
            enrolment.RegistrationNumber = enrolment.RegistrationNumber;
        }
    }
}