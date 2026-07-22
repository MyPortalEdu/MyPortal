using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Contracts.Models.People.Contacts;
using MyPortal.Contracts.Models.People.Students;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;

namespace MyPortal.Data.Repositories;

public class StudentContactRelationshipRepository(
    IDbConnectionFactory factory,
    IAuthorizationService authorizationService)
    : EntityRepository<StudentContactRelationship>(factory, authorizationService),
        IStudentContactRelationshipRepository
{
    public async Task<IReadOnlyList<StudentContactRelationshipResponse>> GetByStudentIdAsync(Guid studentId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition("[dbo].[usp_student_contact_relationship_get_by_student_id]",
                new { studentId }, transaction, commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var rows = await conn.QueryAsync<StudentContactRelationshipResponse>(command);

            return rows.AsList();
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<IReadOnlyList<SiblingResponse>> GetSiblingsByStudentIdAsync(Guid studentId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition("[dbo].[usp_student_get_siblings_by_id]", new { studentId },
                transaction, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

            var rows = await conn.QueryAsync<SiblingResponse>(command);

            return rows.AsList();
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }

    public async Task<IReadOnlyList<ContactStudentResponse>> GetByContactIdAsync(Guid contactId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null)
    {
        var (conn, owns) = AcquireConnection(transaction);

        try
        {
            var command = new CommandDefinition("[dbo].[usp_student_contact_relationship_get_by_contact_id]",
                new { contactId }, transaction, commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var rows = await conn.QueryAsync<ContactStudentResponse>(command);

            return rows.AsList();
        }
        finally
        {
            if (owns)
            {
                conn.Dispose();
            }
        }
    }
}
