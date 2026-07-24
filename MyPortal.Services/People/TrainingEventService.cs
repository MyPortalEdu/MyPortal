using Microsoft.Extensions.Logging;
using MyPortal.Auth.Constants;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Constants;
using MyPortal.Common.Enums;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces.People;
using MyPortal.Services.Interfaces.Providers;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

public class TrainingEventService(
    IAuthorizationService authorizationService,
    ILogger<TrainingEventService> logger,
    ITrainingEventRepository trainingEventRepository,
    IDiaryEventRepository diaryEventRepository,
    IStaffMemberRepository staffMemberRepository,
    IUnitOfWorkFactory unitOfWorkFactory,
    IDateTimeProvider dateTimeProvider)
    : BaseService(authorizationService, logger), ITrainingEventService
{
    // The seeded "Completed" training-certificate status.
    private static readonly Guid CompletedStatusId = Guid.Parse("28F35A25-62FC-4944-96BA-D55A956243AA");

    public async Task<IReadOnlyList<TrainingEventSummaryResponse>> ListAsync(DateTime? from, DateTime? to,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffProfessionalDetails,
            cancellationToken);

        var today = dateTimeProvider.UtcNow.Date;
        var start = (from ?? today.AddMonths(-6)).Date;
        var end = (to ?? today.AddMonths(6)).Date.AddDays(1);
        return await trainingEventRepository.GetSummariesAsync(start, end, cancellationToken);
    }

    public async Task<TrainingEventDetailsResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.ViewAllStaffProfessionalDetails,
            cancellationToken);

        var details = await trainingEventRepository.GetDetailsAsync(id, cancellationToken);
        if (details == null) return null;

        var entity = await trainingEventRepository.GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            details.Attendees =
                (await trainingEventRepository.GetAttendeesAsync(entity.DiaryEventId, cancellationToken))
                .ToList();
        }

        return details;
    }

    public async Task<Guid> CreateAsync(TrainingEventUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffProfessionalDetails,
            cancellationToken);

        return await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var diaryEvent = new DiaryEvent
            {
                Id = SqlConvention.SequentialGuid(),
                Kind = DiaryEventKind.TrainingEvent,
                Subject = model.Title!,
                Description = model.Notes,
                Location = model.Location,
                StartTime = model.StartTime,
                EndTime = model.EndTime ?? model.StartTime,
                IsAllDay = false,
                IsPublic = false,
                // User-managed (unlike holiday events), so it stays editable via the repository guard.
                IsSystem = false
            };
            await diaryEventRepository.InsertAsync(diaryEvent, cancellationToken, uow.Transaction);

            var trainingEvent = new TrainingEvent
            {
                Id = SqlConvention.SequentialGuid(),
                DiaryEventId = diaryEvent.Id,
                TrainingCourseId = model.TrainingCourseId,
                Trainer = model.Trainer,
                Provider = model.Provider,
                Hours = model.Hours,
                Capacity = model.Capacity
            };
            await trainingEventRepository.InsertAsync(trainingEvent, cancellationToken, uow.Transaction);

            return trainingEvent.Id;
        }, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, TrainingEventUpsertRequest model, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffProfessionalDetails,
            cancellationToken);

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            var trainingEvent = await trainingEventRepository.GetByIdAsync(id, cancellationToken, uow.Transaction)
                ?? throw new NotFoundException("Training event not found.");
            var diaryEvent =
                await diaryEventRepository.GetByIdAsync(trainingEvent.DiaryEventId, cancellationToken, uow.Transaction)
                ?? throw new NotFoundException("Training event not found.");

            diaryEvent.Subject = model.Title!;
            diaryEvent.Description = model.Notes;
            diaryEvent.Location = model.Location;
            diaryEvent.StartTime = model.StartTime;
            diaryEvent.EndTime = model.EndTime ?? model.StartTime;
            await diaryEventRepository.UpdateAsync(diaryEvent, cancellationToken, uow.Transaction);

            trainingEvent.TrainingCourseId = model.TrainingCourseId;
            trainingEvent.Trainer = model.Trainer;
            trainingEvent.Provider = model.Provider;
            trainingEvent.Hours = model.Hours;
            trainingEvent.Capacity = model.Capacity;
            await trainingEventRepository.UpdateAsync(trainingEvent, cancellationToken, uow.Transaction);

        }, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffProfessionalDetails,
            cancellationToken);

        var trainingEvent = await trainingEventRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Training event not found.");

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await trainingEventRepository.PurgeAsync(id, trainingEvent.DiaryEventId, cancellationToken,
                uow.Transaction);
        }, cancellationToken);
    }

    public async Task BookAttendeesAsync(Guid id, IEnumerable<Guid> staffMemberIds,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffProfessionalDetails,
            cancellationToken);

        var trainingEvent = await trainingEventRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Training event not found.");

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            foreach (var staffMemberId in staffMemberIds.Distinct())
            {
                var staff = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken,
                    uow.Transaction);
                if (staff == null) continue;
                await trainingEventRepository.BookAttendeeAsync(trainingEvent.DiaryEventId, staff.PersonId,
                    cancellationToken, uow.Transaction);
            }
        }, cancellationToken);
    }

    public async Task RemoveAttendeeAsync(Guid id, Guid staffMemberId, CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffProfessionalDetails,
            cancellationToken);

        var trainingEvent = await trainingEventRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Training event not found.");
        var staff = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken)
            ?? throw new NotFoundException("Staff member not found.");

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await trainingEventRepository.RemoveAttendeeAsync(trainingEvent.DiaryEventId, staff.PersonId,
                cancellationToken, uow.Transaction);
            await trainingEventRepository.DeleteAttendanceCertificateAsync(id, staffMemberId,
                cancellationToken, uow.Transaction);
        }, cancellationToken);
    }

    public async Task SetAttendanceAsync(Guid id, Guid staffMemberId, bool attended,
        CancellationToken cancellationToken)
    {
        await AuthorizationService.RequirePermissionAsync(Permissions.Staff.EditAllStaffProfessionalDetails,
            cancellationToken);

        var details = await trainingEventRepository.GetDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Training event not found.");
        var trainingEvent = await trainingEventRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Training event not found.");
        var staff = await staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken)
            ?? throw new NotFoundException("Staff member not found.");

        await unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await trainingEventRepository.SetAttendedAsync(trainingEvent.DiaryEventId, staff.PersonId, attended,
                cancellationToken, uow.Transaction);

            if (attended)
            {
                await trainingEventRepository.EnsureAttendanceCertificateAsync(id, staffMemberId,
                    details.TrainingCourseId, CompletedStatusId, details.StartTime, details.Hours,
                    details.Provider, cancellationToken, uow.Transaction);
            }
            else
            {
                await trainingEventRepository.DeleteAttendanceCertificateAsync(id, staffMemberId,
                    cancellationToken, uow.Transaction);
            }

        }, cancellationToken);
    }
}
