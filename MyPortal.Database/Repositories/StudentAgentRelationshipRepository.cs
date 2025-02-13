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
    public class StudentAgentRelationshipRepository : BaseReadWriteRepository<StudentAgentRelationship>,
        IStudentAgentRelationshipRepository
    {
        public StudentAgentRelationshipRepository(DbUserWithContext dbUser) : base(dbUser)
        {
        }

        protected override Query JoinRelated(Query query)
        {
            query.LeftJoin("Students as S", "S.Id", $"{TblAlias}.StudentId");
            query.LeftJoin("Agents as A", "A.Id", $"{TblAlias}.AgentId");
            query.LeftJoin("RelationshipTypes as RT", "RT.Id", $"{TblAlias}.RelationshipTypeId");

            return query;
        }

        protected override Query SelectAllRelated(Query query)
        {
            query.SelectAllColumns(typeof(Student), "S");
            query.SelectAllColumns(typeof(Agent), "A");
            query.SelectAllColumns(typeof(RelationshipType), "RT");

            return query;
        }

        protected override async Task<IEnumerable<StudentAgentRelationship>> ExecuteQuery(Query query)
        {
            var sql = Compiler.Compile(query);

            var relationships =
                await DbUser.Transaction.Connection
                    .QueryAsync<StudentAgentRelationship, Student, Agent, RelationshipType, StudentAgentRelationship>(
                        sql.Sql,
                        (sar, student, agent, type) =>
                        {
                            sar.Student = student;
                            sar.Agent = agent;
                            sar.RelationshipType = type;

                            return sar;
                        }, sql.NamedBindings, DbUser.Transaction);

            return relationships;
        }

        public async Task Update(StudentAgentRelationship entity)
        {
            var relationship =
                await DbUser.Context.StudentAgentRelationships.FirstOrDefaultAsync(x => x.Id == entity.Id);

            if (relationship == null)
            {
                throw new EntityNotFoundException("Relationship not found.");
            }

            relationship.RelationshipTypeId = entity.RelationshipTypeId;
        }
    }
}