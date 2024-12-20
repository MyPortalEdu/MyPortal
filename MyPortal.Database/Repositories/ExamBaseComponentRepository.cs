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
    public class ExamBaseComponentRepository : BaseReadWriteRepository<ExamBaseComponent>, IExamBaseComponentRepository
    {
        public ExamBaseComponentRepository(DbUserWithContext dbUser) : base(dbUser)
        {
        }

        protected override Query JoinRelated(Query query)
        {
            query.LeftJoin("ExamAssessmentModes", "EAM.Id", $"{TblAlias}.AssessmentModeId");
            query.LeftJoin("ExamAssessments as EA", "EA.Id", $"{TblAlias}.ExamAssessmentId");

            return query;
        }

        protected override Query SelectAllRelated(Query query)
        {
            query.SelectAllColumns(typeof(ExamAssessmentMode), "EAM");
            query.SelectAllColumns(typeof(ExamAssessment), "EA");

            return query;
        }

        protected override async Task<IEnumerable<ExamBaseComponent>> ExecuteQuery(Query query)
        {
            var sql = Compiler.Compile(query);

            var examBaseComponents =
                await DbUser.Transaction.Connection
                    .QueryAsync<ExamBaseComponent, ExamAssessmentMode, ExamAssessment, ExamBaseComponent>(sql.Sql,
                        (component, mode, assessment) =>
                        {
                            component.AssessmentMode = mode;
                            component.Assessment = assessment;

                            return component;
                        }, sql.NamedBindings, DbUser.Transaction);

            return examBaseComponents;
        }

        public async Task Update(ExamBaseComponent entity)
        {
            var baseComponent = await DbUser.Context.ExamBaseComponents.FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (baseComponent == null)
            {
                throw new EntityNotFoundException("Base component not found.");
            }

            baseComponent.ComponentCode = entity.ComponentCode;
        }
    }
}