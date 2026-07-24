using System.Data;
using Dapper;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Data.Repositories.Base;
using MyPortal.Data.Utilities;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Data.Repositories;

public class TrainingEventRepository(IDbConnectionFactory factory, IAuthorizationService authorizationService)
    : EntityRepository<TrainingEvent>(factory, authorizationService), ITrainingEventRepository
{
    public async Task<IReadOnlyList<TrainingEventSummaryResponse>> GetSummariesAsync(DateTime from,
        DateTime to, CancellationToken cancellationToken)
    {
        var sql = SqlResourceLoader.Load("People.GetTrainingEventSummaries.sql");
        var (conn, owns) = AcquireConnection(null);
        try
        {
            var rows = await conn.QueryAsync<TrainingEventSummaryResponse>(
                new CommandDefinition(sql, new { from, to }, cancellationToken: cancellationToken));
            return rows.AsList();
        }
        finally { if (owns) conn.Dispose(); }
    }

    public async Task<TrainingEventDetailsResponse?> GetDetailsAsync(Guid id,
        CancellationToken cancellationToken)
    {
        var sql = SqlResourceLoader.Load("People.GetTrainingEventDetails.sql");
        var (conn, owns) = AcquireConnection(null);
        try
        {
            return await conn.QuerySingleOrDefaultAsync<TrainingEventDetailsResponse>(
                new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken));
        }
        finally { if (owns) conn.Dispose(); }
    }

    public async Task<IReadOnlyList<TrainingEventAttendeeResponse>> GetAttendeesAsync(Guid diaryEventId,
        CancellationToken cancellationToken)
    {
        var sql = SqlResourceLoader.Load("People.GetTrainingEventAttendees.sql");
        var (conn, owns) = AcquireConnection(null);
        try
        {
            var rows = await conn.QueryAsync<TrainingEventAttendeeResponse>(
                new CommandDefinition(sql, new { diaryEventId }, cancellationToken: cancellationToken));
            return rows.AsList();
        }
        finally { if (owns) conn.Dispose(); }
    }

    public Task BookAttendeeAsync(Guid diaryEventId, Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null) =>
        ExecuteAsync(@"
INSERT INTO [dbo].[DiaryEventAttendees] ([Id],[EventId],[PersonId],[ResponseId],[IsRequired],[HasAttended],[CanEditEvent])
SELECT NEWID(), @diaryEventId, @personId, NULL, 1, NULL, 0
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[DiaryEventAttendees] WHERE [EventId]=@diaryEventId AND [PersonId]=@personId);",
            new { diaryEventId, personId }, cancellationToken, transaction);

    public Task RemoveAttendeeAsync(Guid diaryEventId, Guid personId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null) =>
        ExecuteAsync("DELETE FROM [dbo].[DiaryEventAttendees] WHERE [EventId]=@diaryEventId AND [PersonId]=@personId;",
            new { diaryEventId, personId }, cancellationToken, transaction);

    public Task SetAttendedAsync(Guid diaryEventId, Guid personId, bool attended,
        CancellationToken cancellationToken, IDbTransaction? transaction = null) =>
        ExecuteAsync("UPDATE [dbo].[DiaryEventAttendees] SET [HasAttended]=@attended WHERE [EventId]=@diaryEventId AND [PersonId]=@personId;",
            new { diaryEventId, personId, attended }, cancellationToken, transaction);

    public Task EnsureAttendanceCertificateAsync(Guid trainingEventId, Guid staffMemberId,
        Guid trainingCourseId, Guid statusId, DateTime completedDate, decimal? hours, string? provider,
        CancellationToken cancellationToken, IDbTransaction? transaction = null) =>
        ExecuteAsync(@"
INSERT INTO [dbo].[TrainingCertificates] ([Id],[TrainingCourseId],[TrainingEventId],[StaffMemberId],[TrainingCertificateStatusId],[CompletedDate],[ExpiryDate],[Provider],[Hours],[CertificateReference])
SELECT NEWID(), @trainingCourseId, @trainingEventId, @staffMemberId, @statusId, @completedDate, NULL, @provider, @hours, NULL
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[TrainingCertificates] WHERE [TrainingEventId]=@trainingEventId AND [StaffMemberId]=@staffMemberId);",
            new { trainingEventId, staffMemberId, trainingCourseId, statusId, completedDate, hours, provider },
            cancellationToken, transaction);

    public Task DeleteAttendanceCertificateAsync(Guid trainingEventId, Guid staffMemberId,
        CancellationToken cancellationToken, IDbTransaction? transaction = null) =>
        ExecuteAsync("DELETE FROM [dbo].[TrainingCertificates] WHERE [TrainingEventId]=@trainingEventId AND [StaffMemberId]=@staffMemberId;",
            new { trainingEventId, staffMemberId }, cancellationToken, transaction);

    public Task PurgeAsync(Guid trainingEventId, Guid diaryEventId, CancellationToken cancellationToken,
        IDbTransaction? transaction = null) =>
        ExecuteAsync(@"
DELETE FROM [dbo].[TrainingCertificates] WHERE [TrainingEventId]=@trainingEventId;
DELETE FROM [dbo].[DiaryEventAttendees] WHERE [EventId]=@diaryEventId;
DELETE FROM [dbo].[TrainingEvents] WHERE [Id]=@trainingEventId;
DELETE FROM [dbo].[DiaryEvents] WHERE [Id]=@diaryEventId;",
            new { trainingEventId, diaryEventId }, cancellationToken, transaction);

    private async Task ExecuteAsync(string sql, object parameters, CancellationToken cancellationToken,
        IDbTransaction? transaction)
    {
        var (conn, owns) = AcquireConnection(transaction);
        try
        {
            await conn.ExecuteAsync(new CommandDefinition(sql, parameters, transaction,
                cancellationToken: cancellationToken));
        }
        finally { if (owns) conn.Dispose(); }
    }
}
