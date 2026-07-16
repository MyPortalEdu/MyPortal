using System.Data;
using Microsoft.Extensions.Logging;
using MyPortal.Auth.Interfaces;
using MyPortal.Common.Exceptions;
using MyPortal.Common.Interfaces;
using MyPortal.Contracts.Models.People;
using MyPortal.Core.Entities;
using MyPortal.Data.Interfaces;
using MyPortal.Services.Extensions;
using MyPortal.Services.Interfaces;
using MyPortal.Services.Interfaces.People;
using QueryKit.Sql;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Services.People;

/// <summary>
/// Owns the Professional Details area: the QTS / induction / teaching-status fields on the staff
/// member plus the structured qualifications child collection. Gating is enforced under
/// <see cref="StaffArea.ProfessionalDetails"/> — Own / Managed / All view, HR or line-manager edit.
/// </summary>
public class StaffProfessionalService : BaseService, IStaffProfessionalService
{
    private readonly IStaffMemberAccessService _accessService;
    private readonly IStaffMemberRepository _staffMemberRepository;
    private readonly IStaffQualificationRepository _qualificationRepository;
    private readonly IQtsRouteRepository _qtsRouteRepository;
    private readonly IInductionStatusRepository _inductionStatusRepository;
    private readonly IQualificationLevelRepository _qualificationLevelRepository;
    private readonly IClassOfDegreeRepository _classOfDegreeRepository;
    private readonly IValidationService _validationService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public StaffProfessionalService(IAuthorizationService authorizationService,
        ILogger<StaffProfessionalService> logger, IStaffMemberAccessService accessService,
        IStaffMemberRepository staffMemberRepository, IStaffQualificationRepository qualificationRepository,
        IQtsRouteRepository qtsRouteRepository, IInductionStatusRepository inductionStatusRepository,
        IQualificationLevelRepository qualificationLevelRepository, IClassOfDegreeRepository classOfDegreeRepository,
        IValidationService validationService, IUnitOfWorkFactory unitOfWorkFactory) : base(authorizationService, logger)
    {
        _accessService = accessService;
        _staffMemberRepository = staffMemberRepository;
        _qualificationRepository = qualificationRepository;
        _qtsRouteRepository = qtsRouteRepository;
        _inductionStatusRepository = inductionStatusRepository;
        _qualificationLevelRepository = qualificationLevelRepository;
        _classOfDegreeRepository = classOfDegreeRepository;
        _validationService = validationService;
        _unitOfWorkFactory = unitOfWorkFactory;
    }

    public async Task<StaffProfessionalDetailsResponse> GetProfessionalDetailsAsync(Guid staffMemberId,
        CancellationToken cancellationToken)
    {
        await _accessService.RequireAsync(staffMemberId, StaffArea.ProfessionalDetails,
            StaffAccess.ViewOwn | StaffAccess.ViewManaged | StaffAccess.ViewAll, cancellationToken);

        var staffMember = await _staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        var qualifications = await _qualificationRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken);

        var qtsRoutes = await _qtsRouteRepository.GetListAsync(cancellationToken: cancellationToken);
        var inductionStatuses = await _inductionStatusRepository.GetListAsync(cancellationToken: cancellationToken);
        var qualificationLevels = await _qualificationLevelRepository.GetListAsync(cancellationToken: cancellationToken);
        var classesOfDegree = await _classOfDegreeRepository.GetListAsync(cancellationToken: cancellationToken);

        return new StaffProfessionalDetailsResponse
        {
            IsTeachingStaff = staffMember.IsTeachingStaff,
            HasQts = staffMember.HasQts,
            HasHlta = staffMember.HasHlta,
            HasQtls = staffMember.HasQtls,
            HasEyts = staffMember.HasEyts,
            IsSeniorLeadership = staffMember.IsSeniorLeadership,
            TeacherReferenceNumber = staffMember.TeacherReferenceNumber,
            QtsRouteId = staffMember.QtsRouteId,
            QtsAwardedDate = staffMember.QtsAwardedDate,
            InductionStatusId = staffMember.InductionStatusId,
            InductionStartDate = staffMember.InductionStartDate,
            InductionCompletedDate = staffMember.InductionCompletedDate,
            QualificationsSummary = staffMember.Qualifications,
            Qualifications = qualifications.Select(q => new StaffQualificationResponse
            {
                Id = q.Id,
                QualificationLevelId = q.QualificationLevelId,
                Title = q.Title,
                Subject = q.Subject,
                AwardingBody = q.AwardingBody,
                Grade = q.Grade,
                ClassOfDegreeId = q.ClassOfDegreeId,
                YearAwarded = q.YearAwarded
            }).ToList(),
            QtsRoutes = qtsRoutes.ToOrderedLookup(),
            InductionStatuses = inductionStatuses.ToAlphabeticalLookup(),
            QualificationLevels = qualificationLevels.ToAlphabeticalLookup(),
            ClassesOfDegree = classesOfDegree.ToOrderedLookup()
        };
    }

    public async Task UpdateProfessionalDetailsAsync(Guid staffMemberId,
        StaffProfessionalDetailsUpsertRequest model, CancellationToken cancellationToken)
    {
        // HR (All) or the line manager (Managed) — no self-edit; the data is HR-verified.
        await _accessService.RequireAsync(staffMemberId, StaffArea.ProfessionalDetails,
            StaffAccess.EditManaged | StaffAccess.EditAll, cancellationToken);

        await _validationService.ValidateAsync(model);

        var staffMember = await _staffMemberRepository.GetByIdAsync(staffMemberId, cancellationToken);

        if (staffMember == null)
        {
            throw new NotFoundException("Staff member not found.");
        }

        staffMember.IsTeachingStaff = model.IsTeachingStaff;
        staffMember.HasQts = model.HasQts;
        staffMember.HasHlta = model.HasHlta;
        staffMember.HasQtls = model.HasQtls;
        staffMember.HasEyts = model.HasEyts;
        staffMember.IsSeniorLeadership = model.IsSeniorLeadership;
        staffMember.TeacherReferenceNumber = model.TeacherReferenceNumber;
        staffMember.QtsRouteId = model.QtsRouteId;
        staffMember.QtsAwardedDate = model.QtsAwardedDate;
        staffMember.InductionStatusId = model.InductionStatusId;
        staffMember.InductionStartDate = model.InductionStartDate;
        staffMember.InductionCompletedDate = model.InductionCompletedDate;
        staffMember.Qualifications = model.QualificationsSummary;

        await _unitOfWorkFactory.RunInTransactionAsync(null, async uow =>
        {
            await _staffMemberRepository.UpdateAsync(staffMember, cancellationToken, uow.Transaction);
            await ReconcileQualificationsAsync(staffMemberId, model.Qualifications, uow.Transaction, cancellationToken);
        }, cancellationToken);
    }

    private async Task ReconcileQualificationsAsync(Guid staffMemberId, List<StaffQualificationUpsertItem> incoming,
        IDbTransaction? transaction, CancellationToken cancellationToken)
    {
        var existing =
            await _qualificationRepository.GetByStaffMemberIdAsync(staffMemberId, cancellationToken, transaction);
        var existingById = existing.ToDictionary(q => q.Id);
        var keptIds = incoming.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();

        // Soft-delete rows the payload dropped.
        foreach (var row in existing)
        {
            if (!keptIds.Contains(row.Id))
            {
                await _qualificationRepository.DeleteAsync(row.Id, cancellationToken, true, transaction);
            }
        }

        foreach (var item in incoming)
        {
            if (item.Id.HasValue && existingById.TryGetValue(item.Id.Value, out var entity))
            {
                entity.QualificationLevelId = item.QualificationLevelId;
                entity.Title = item.Title;
                entity.Subject = item.Subject;
                entity.AwardingBody = item.AwardingBody;
                entity.Grade = item.Grade;
                entity.ClassOfDegreeId = item.ClassOfDegreeId;
                entity.YearAwarded = item.YearAwarded;
                await _qualificationRepository.UpdateAsync(entity, cancellationToken, transaction);
            }
            else
            {
                // Null id, or an id we don't own — treat as a new row rather than trusting it.
                await _qualificationRepository.InsertAsync(new StaffQualification
                {
                    Id = SqlConvention.SequentialGuid(),
                    StaffMemberId = staffMemberId,
                    QualificationLevelId = item.QualificationLevelId,
                    Title = item.Title,
                    Subject = item.Subject,
                    AwardingBody = item.AwardingBody,
                    Grade = item.Grade,
                    ClassOfDegreeId = item.ClassOfDegreeId,
                    YearAwarded = item.YearAwarded
                }, cancellationToken, transaction);
            }
        }
    }
}
