using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyPortal.Database.Constants;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Database.Models.Entity;
using MyPortal.Database.Models.Search;
using MyPortal.Database.Repositories;
using MyPortal.Logic.Exceptions;
using MyPortal.Logic.Extensions;
using MyPortal.Logic.Interfaces;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.Behaviour.Achievements;
using MyPortal.Logic.Models.Data.Behaviour.Detentions;
using MyPortal.Logic.Models.Data.Behaviour.Incidents;
using MyPortal.Logic.Models.Data.Students;
using MyPortal.Logic.Models.Requests.Behaviour.Achievements;
using MyPortal.Logic.Models.Requests.Behaviour.Detentions;
using MyPortal.Logic.Models.Requests.Behaviour.Incidents;
using MyPortal.Logic.Models.Summary;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Logic.Services
{
    public sealed class BehaviourService : BaseServiceWithAccessControl, IBehaviourService
    {
        public BehaviourService(ISessionUser user, IUserService userService, IPersonService personService,
            IStudentService studentService) : base(user, userService, personService, studentService)
        {
        }

        public async Task<IEnumerable<StudentAchievementSummaryModel>> GetAchievementsByStudent(Guid studentId,
            Guid academicYearId)
        {
            await using var unitOfWork = await User.GetConnection();

            var student = await unitOfWork.GetRepository<IStudentRepository>().GetById(studentId);

            await VerifyAccessToPerson(student.PersonId);

            var achievements =
                (await unitOfWork.GetRepository<IStudentAchievementRepository>()
                    .GetByStudent(studentId, academicYearId)).Select(a =>
                    new StudentAchievementModel(a));

            var summaries = new List<StudentAchievementSummaryModel>();

            foreach (var achievementModel in achievements)
            {
                // TODO: This should have its own query as the navigation properties are likely no longer available
                summaries.Add(new StudentAchievementSummaryModel(achievementModel));
            }

            return summaries;
        }

        public async Task<StudentAchievementModel> GetStudentAchievementById(Guid achievementId)
        {
            await using var unitOfWork = await User.GetConnection();

            var achievement = await unitOfWork.GetRepository<IStudentAchievementRepository>().GetById(achievementId);

            var student = await unitOfWork.GetRepository<IStudentRepository>().GetById(achievement.StudentId);

            await VerifyAccessToPerson(student.PersonId);

            return new StudentAchievementModel(achievement);
        }

        public async Task<int> GetAchievementPointsByStudent(Guid studentId, Guid academicYearId)
        {
            await using var unitOfWork = await User.GetConnection();

            var points = await unitOfWork.GetRepository<IStudentAchievementRepository>()
                .GetPointsByStudent(studentId, academicYearId);

            return points;
        }

        public async Task<int> GetAchievementCountByStudent(Guid studentId, Guid academicYearId)
        {
            await using var unitOfWork = await User.GetConnection();

            var count = await unitOfWork.GetRepository<IStudentAchievementRepository>()
                .GetCountByStudent(studentId, academicYearId);

            return count;
        }

        public async Task<StudentAchievementModel> CreateAchievement(AchievementRequestModel achievement)
        {
            Validate(achievement);

            var userId = User.GetUserId();

            if (userId.HasValue)
            {
                await using var unitOfWork = await User.GetConnection();

                var now = DateTime.Now;

                var academicYearService = new AcademicYearService(User);
                await academicYearService.IsAcademicYearLocked(achievement.AcademicYearId);

                var model = new StudentAchievement
                {
                    Id = Guid.NewGuid(),
                    StudentId = achievement.StudentId,
                    OutcomeId = achievement.OutcomeId,
                    Points = achievement.Points,
                    Achievement = new Achievement
                    {
                        Id = Guid.NewGuid(),
                        AcademicYearId = achievement.AcademicYearId,
                        AchievementTypeId = achievement.AchievementTypeId,
                        LocationId = achievement.LocationId,
                        Comments = achievement.Comments,
                        CreatedById = userId.Value,
                        CreatedDate = now
                    }
                };

                unitOfWork.GetRepository<IStudentAchievementRepository>().Create(model);

                await unitOfWork.SaveChangesAsync();

                return new StudentAchievementModel(model);
            }

            throw Unauthenticated();
        }

        public async Task UpdateAchievement(Guid achievementId, AchievementRequestModel achievement)
        {
            Validate(achievement);

            await using var unitOfWork = await User.GetConnection();

            var achievementInDb = await unitOfWork.GetRepository<IStudentAchievementRepository>().GetById(achievementId);

            if (achievementInDb == null)
            {
                throw new NotFoundException("Achievement not found.");
            }

            var academicYearService = new AcademicYearService(User);
            await academicYearService.IsAcademicYearLocked(achievementInDb.Achievement.AcademicYearId);

            achievementInDb.Achievement.AchievementTypeId = achievement.AchievementTypeId;
            achievementInDb.Achievement.LocationId = achievement.LocationId;
            achievementInDb.OutcomeId = achievement.OutcomeId;
            achievementInDb.Achievement.Comments = achievement.Comments;
            achievementInDb.Points = achievement.Points;

            await unitOfWork.GetRepository<IStudentAchievementRepository>().Update(achievementInDb);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAchievement(Guid achievementId)
        {
            var achievement = await GetStudentAchievementById(achievementId);

            var academicYearService = new AcademicYearService(User);
            await academicYearService.IsAcademicYearLocked(achievement.Achievement.AcademicYearId);

            await using var unitOfWork = await User.GetConnection();

            await unitOfWork.GetRepository<IAchievementRepository>().Delete(achievementId);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<AchievementTypeModel>> GetAchievementTypes()
        {
            await using var unitOfWork = await User.GetConnection();
            var types = await unitOfWork.GetRepository<IAchievementTypeRepository>().GetAll();

            return types.Select(t => new AchievementTypeModel(t)).ToList();
        }

        public async Task<IEnumerable<AchievementOutcomeModel>> GetAchievementOutcomes()
        {
            await using var unitOfWork = await User.GetConnection();
            var outcomes = await unitOfWork.GetRepository<IAchievementOutcomeRepository>().GetAll();

            return outcomes.Select(o => new AchievementOutcomeModel(o)).ToList();
        }

        public async Task<IEnumerable<StudentIncidentSummaryModel>> GetIncidentsByStudent(Guid studentId,
            Guid academicYearId)
        {
            await using var unitOfWork = await User.GetConnection();

            var student = await unitOfWork.GetRepository<IStudentRepository>().GetById(studentId);

            await VerifyAccessToPerson(student.PersonId);

            var incidents = await unitOfWork.GetRepository<IStudentIncidentRepository>()
                .GetByStudent(studentId, academicYearId);

            var models = incidents.Select(i => new StudentIncidentModel(i));

            var results = new List<StudentIncidentSummaryModel>();

            foreach (var model in models)
            {
                // TODO: May need addressing
                results.Add(new StudentIncidentSummaryModel(model));
            }

            return results;
        }

        public async Task<StudentIncidentModel> GetIncidentById(Guid incidentId)
        {
            await using var unitOfWork = await User.GetConnection();
            var incident = await unitOfWork.GetRepository<IStudentIncidentRepository>().GetById(incidentId);

            var student = await unitOfWork.GetRepository<IStudentRepository>().GetById(incident.StudentId);

            await VerifyAccessToPerson(student.PersonId);

            return new StudentIncidentModel(incident);
        }

        public async Task<int> GetBehaviourPointsByStudent(Guid studentId, Guid academicYearId)
        {
            await using var unitOfWork = await User.GetConnection();
            var points = await unitOfWork.GetRepository<IStudentIncidentRepository>()
                .GetPointsByStudent(studentId, academicYearId);

            return points;
        }

        public async Task<int> GetBehaviourCountByStudent(Guid studentId, Guid academicYearId)
        {
            await using var unitOfWork = await User.GetConnection();
            var count = await unitOfWork.GetRepository<IStudentIncidentRepository>()
                .GetCountByStudent(studentId, academicYearId);

            return count;
        }

        public async Task<StudentIncidentModel> CreateIncident(IncidentRequestModel incident)
        {
            Validate(incident);

            var userId = User.GetUserId();

            if (userId != null)
            {
                var studentIncident = new StudentIncident
                {
                    Id = Guid.NewGuid(),
                    Points = incident.Points,
                    OutcomeId = incident.OutcomeId,
                    StatusId = incident.StatusId,
                    StudentId = incident.StudentId,
                    Incident = new Incident
                    {
                        Id = Guid.NewGuid(),
                        CreatedDate = DateTime.Now,
                        BehaviourTypeId = incident.BehaviourTypeId,
                        LocationId = incident.LocationId,
                        CreatedById = userId.Value,
                        Comments = incident.Comments,
                        AcademicYearId = incident.AcademicYearId,
                    }
                };

                foreach (var detentionId in incident.DetentionIds)
                {
                    studentIncident.LinkedDetentions.Add(new StudentDetention
                    {
                        StudentId = incident.StudentId,
                        DetentionId = detentionId
                    });
                }

                await using var unitOfWork = await User.GetConnection();

                unitOfWork.GetRepository<IStudentIncidentRepository>().Create(studentIncident);

                await unitOfWork.SaveChangesAsync();

                return new StudentIncidentModel(studentIncident);
            }

            throw Unauthenticated();
        }

        public async Task UpdateIncident(Guid incidentId, IncidentRequestModel incident)
        {
            Validate(incident);

            await using var unitOfWork = await User.GetConnection();

            var studentIncidentInDb = await unitOfWork.GetRepository<IStudentIncidentRepository>().GetById(incidentId);

            if (studentIncidentInDb == null)
            {
                throw new NotFoundException("Student incident not found.");
            }

            studentIncidentInDb.Points = incident.Points;
            studentIncidentInDb.Incident.BehaviourTypeId = incident.BehaviourTypeId;
            studentIncidentInDb.Incident.LocationId = incident.LocationId;
            studentIncidentInDb.OutcomeId = incident.OutcomeId;
            studentIncidentInDb.StatusId = incident.StatusId;
            studentIncidentInDb.Incident.Comments = incident.Comments;

            await unitOfWork.GetRepository<IStudentIncidentRepository>().Update(studentIncidentInDb);

            var linkedDetentions = await unitOfWork.GetRepository<IStudentDetentionRepository>()
                .GetByStudentIncident(studentIncidentInDb.Id);

            var detentionsToAdd = incident.DetentionIds.Where(d => linkedDetentions.All(ld => ld.DetentionId != d))
                .ToArray();

            var detentionsToRemove =
                linkedDetentions.Where(ld => incident.DetentionIds.All(d => ld.DetentionId != d))
                    .Select(ld => ld.DetentionId).ToArray();

            await RemoveDetentions(studentIncidentInDb.Id, detentionsToRemove);

            await AddDetentions(studentIncidentInDb.Id, detentionsToAdd);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteIncident(Guid incidentId)
        {
            await using var unitOfWork = await User.GetConnection();

            await unitOfWork.GetRepository<IIncidentRepository>().Delete(incidentId);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<IncidentTypeModel>> GetIncidentTypes()
        {
            await using var unitOfWork = await User.GetConnection();

            var types = await unitOfWork.GetRepository<IIncidentTypeRepository>().GetAll();

            return types.Select(t => new IncidentTypeModel(t)).ToList();
        }

        public async Task<IEnumerable<BehaviourRoleTypeModel>> GetRoleTypes()
        {
            await using var unitOfWork = await User.GetConnection();

            var roleTypes = await unitOfWork.GetRepository<IBehaviourRoleTypeRepository>().GetAll();

            return roleTypes.Select(r => new BehaviourRoleTypeModel(r)).ToList();
        }

        public async Task<IEnumerable<BehaviourOutcomeModel>> GetIncidentOutcomes()
        {
            await using var unitOfWork = await User.GetConnection();

            var outcomes = await unitOfWork.GetRepository<IBehaviourOutcomeRepository>().GetAll();

            return outcomes.Select(o => new BehaviourOutcomeModel(o)).ToList();
        }

        public async Task<IEnumerable<BehaviourStatusModel>> GetBehaviourStatus()
        {
            await using var unitOfWork = await User.GetConnection();

            var status = await unitOfWork.GetRepository<IBehaviourStatusRepository>().GetAll();

            return status.Select(s => new BehaviourStatusModel(s)).ToList();
        }

        public async Task<IEnumerable<DetentionModel>> GetDetentions(DetentionSearchOptions searchOptions)
        {
            await using var unitOfWork = await User.GetConnection();

            var detentions = await unitOfWork.GetRepository<IDetentionRepository>().GetAll(searchOptions);

            return detentions.Select(d => new DetentionModel(d));
        }

        public async Task<DetentionModel> GetDetentionById(Guid detentionId)
        {
            await using var unitOfWork = await User.GetConnection();

            var detention = await unitOfWork.GetRepository<IDetentionRepository>().GetById(detentionId);

            return new DetentionModel(detention);
        }

        public async Task<DetentionModel> GetDetentionByIncident(Guid incidentId)
        {
            await using var unitOfWork = await User.GetConnection();

            var detention = await unitOfWork.GetRepository<IDetentionRepository>().GetByIncident(incidentId);

            return new DetentionModel(detention);
        }

        public async Task<DetentionModel> CreateDetention(DetentionRequestModel model)
        {
            Validate(model);

            var detention = new Detention
            {
                Id = Guid.NewGuid(),
                DetentionTypeId = model.DetentionTypeId,
                SupervisorId = model.SupervisorId,
                Event = new DiaryEvent
                {
                    Id = Guid.NewGuid(),
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    RoomId = model.RoomId,
                    EventTypeId = EventTypes.Detention,
                    Subject = "Detention"
                }
            };

            await using var unitOfWork = await User.GetConnection();

            unitOfWork.GetRepository<IDetentionRepository>().Create(detention);

            DateTime? nextOccurrence = model.StartTime.GetNextOccurrence(model.Frequency);
            TimeSpan duration = model.EndTime - model.StartTime;

            while (nextOccurrence != null && nextOccurrence.Value < model.LastOccurrence)
            {
                var nextDetention = new Detention
                {
                    DetentionTypeId = model.DetentionTypeId,
                    SupervisorId = model.SameSupervisor ? model.SupervisorId : null,
                    Event = new DiaryEvent
                    {
                        Id = Guid.NewGuid(),
                        StartTime = nextOccurrence.Value,
                        EndTime = nextOccurrence.Value.Add(duration),
                        RoomId = model.RoomId,
                        EventTypeId = EventTypes.Detention,
                        Subject = "Detention"
                    }
                };

                unitOfWork.GetRepository<IDetentionRepository>().Create(nextDetention);
                await unitOfWork.BatchSaveChangesAsync();

                nextOccurrence = nextOccurrence.Value.GetNextOccurrence(model.Frequency);
            }

            await unitOfWork.SaveChangesAsync();

            return new DetentionModel(detention);
        }

        public async Task UpdateDetention(Guid detentionId, DetentionRequestModel detention)
        {
            Validate(detention);

            await using var unitOfWork = await User.GetConnection();

            var detentionInDb = await unitOfWork.GetRepository<IDetentionRepository>().GetById(detentionId);

            detentionInDb.DetentionTypeId = detention.DetentionTypeId;
            detentionInDb.Event.StartTime = detention.StartTime;
            detentionInDb.Event.EndTime = detention.EndTime;
            detentionInDb.Event.RoomId = detention.RoomId;

            await unitOfWork.GetRepository<IDetentionRepository>().Update(detentionInDb);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteDetention(Guid detentionId)
        {
            await using var unitOfWork = await User.GetConnection();

            await unitOfWork.GetRepository<IDetentionRepository>().Delete(detentionId);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<StudentIncidentSummaryModel>> GetInvolvedStudentsByIncident(Guid incidentId)
        {
            await using var unitOfWork = await User.GetConnection();

            var involvedStudents =
                (await unitOfWork.GetRepository<IStudentIncidentRepository>().GetByIncident(incidentId)).Select(s =>
                    new StudentIncidentModel(s)).ToList();

            var results = new List<StudentIncidentSummaryModel>();

            foreach (var involvedStudent in involvedStudents)
            {
                results.Add(new StudentIncidentSummaryModel(involvedStudent));
            }

            return results;
        }

        public async Task AddStudentToIncident(StudentIncidentRequestModel model)
        {
            Validate(model);

            var studentIncident = new StudentIncident
            {
                Id = Guid.NewGuid(),
                IncidentId = model.IncidentId,
                OutcomeId = model.OutcomeId,
                StatusId = model.StatusId,
                RoleTypeId = model.RoleTypeId,
                StudentId = model.StudentId,
                Points = model.Points
            };

            await using var unitOfWork = await User.GetConnection();

            unitOfWork.GetRepository<IStudentIncidentRepository>().Create(studentIncident);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveStudentFromIncident(Guid studentIncidentId)
        {
            await using var unitOfWork = await User.GetConnection();

            var studentIncident = await unitOfWork.GetRepository<IStudentIncidentRepository>().GetById(studentIncidentId);

            if (studentIncident == null)
            {
                throw new NotFoundException("Student incident not found.");
            }

            var studentCount = await unitOfWork.GetRepository<IStudentIncidentRepository>()
                .GetCountByIncident(studentIncident.IncidentId);

            if (studentCount < 2)
            {
                throw new LogicException("Cannot remove the only student from this incident.");
            }

            await unitOfWork.GetRepository<IStudentIncidentRepository>().Delete(studentIncidentId);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task AddDetentions(Guid studentIncidentId, Guid[] detentionIds)
        {
            await using var unitOfWork = await User.GetConnection();

            var studentIncident = await unitOfWork.GetRepository<IStudentIncidentRepository>().GetById(studentIncidentId);

            if (studentIncident == null)
            {
                throw new NotFoundException("Student incident not found.");
            }

            foreach (var detentionId in detentionIds)
            {
                var incidentDetention = new StudentDetention
                {
                    StudentId = studentIncident.StudentId,
                    DetentionId = detentionId
                };

                unitOfWork.GetRepository<IStudentDetentionRepository>().Create(incidentDetention);
            }

            await unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveDetentions(Guid studentIncidentId, Guid[] detentionIds)
        {
            await using var unitOfWork = await User.GetConnection();

            foreach (var detentionId in detentionIds)
            {
                var relatedIncident =
                    await unitOfWork.GetRepository<IStudentDetentionRepository>()
                        .GetStudentDetention(detentionId, studentIncidentId);

                if (relatedIncident == null)
                {
                    throw new NotFoundException("Detention not found.");
                }

                await unitOfWork.GetRepository<IStudentDetentionRepository>().Delete(relatedIncident.Id);
            }

            await unitOfWork.SaveChangesAsync();
        }
    }
}