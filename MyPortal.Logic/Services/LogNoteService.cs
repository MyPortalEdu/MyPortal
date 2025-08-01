using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyPortal.Database.Constants;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Exceptions;
using MyPortal.Logic.Interfaces;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.Students;
using MyPortal.Logic.Models.Requests.Student.LogNotes;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Logic.Services
{
    public sealed class LogNoteService : BaseServiceWithAccessControl, ILogNoteService
    {
        public LogNoteService(ISessionUser user, IUserService userService, IPersonService personService,
            IStudentService studentService) : base(user, userService, personService, studentService)
        {
        }

        public async Task<LogNoteModel> GetLogNoteById(Guid logNoteId)
        {
            await using var unitOfWork = await User.GetConnection();

            var logNote = await unitOfWork.GetRepository<ILogNoteRepository>().GetById(logNoteId);

            if (logNote == null)
            {
                throw new NotFoundException("Log note not found.");
            }

            if (logNote.Private && !User.IsType(UserTypes.Staff))
            {
                throw new PermissionException("You do not have access to this log note.");
            }
            
            var student = await unitOfWork.GetRepository<IStudentRepository>().GetById(logNote.StudentId);

            await VerifyAccessToPerson(student.PersonId);

            return new LogNoteModel(logNote);
        }

        public async Task<IEnumerable<LogNoteModel>> GetLogNotesByStudent(Guid studentId, Guid academicYearId)
        {
            await using var unitOfWork = await User.GetConnection();
            
            var student = await unitOfWork.GetRepository<IStudentRepository>().GetById(studentId);
            
            await VerifyAccessToPerson(student.PersonId);
            
            var includePrivate = User.IsType(UserTypes.Staff);

            var logNotes =
                await unitOfWork.GetRepository<ILogNoteRepository>()
                    .GetByStudent(studentId, academicYearId, includePrivate);

            return logNotes.OrderByDescending(n => n.CreatedDate)
                .Select(l => new LogNoteModel(l)).ToList();
        }

        public async Task<IEnumerable<LogNoteTypeModel>> GetLogNoteTypes()
        {
            await using var unitOfWork = await User.GetConnection();

            var logNoteTypes = await unitOfWork.GetRepository<ILogNoteTypeRepository>().GetAll();

            return logNoteTypes.Select(t => new LogNoteTypeModel(t));
        }

        public async Task CreateLogNote(LogNoteRequestModel logNoteModel)
        {
            Validate(logNoteModel);

            var userId = User.GetUserId();

            if (userId != null)
            {
                await using var unitOfWork = await User.GetConnection();

                var academicYearService = new AcademicYearService(User);
                await academicYearService.IsAcademicYearLocked(logNoteModel.AcademicYearId);

                var createDate = DateTime.Now;

                var logNote = new LogNote
                {
                    Id = Guid.NewGuid(),
                    TypeId = logNoteModel.TypeId,
                    Message = logNoteModel.Message,
                    StudentId = logNoteModel.StudentId,
                    CreatedDate = createDate,
                    CreatedById = userId.Value,
                    AcademicYearId = logNoteModel.AcademicYearId
                };

                unitOfWork.GetRepository<ILogNoteRepository>().Create(logNote);

                await unitOfWork.SaveChangesAsync();
            }
            else
            {
                throw Unauthenticated();
            }
        }

        public async Task UpdateLogNote(Guid logNoteId, LogNoteRequestModel logNoteModel)
        {
            Validate(logNoteModel);

            await using var unitOfWork = await User.GetConnection();

            var logNote = await unitOfWork.GetRepository<ILogNoteRepository>().GetById(logNoteId);

            var academicYearService = new AcademicYearService(User);
            await academicYearService.IsAcademicYearLocked(logNote.AcademicYearId);

            if (logNote == null)
            {
                throw new NotFoundException("Log note not found.");
            }

            logNote.TypeId = logNoteModel.TypeId;
            logNote.Message = logNoteModel.Message;

            await unitOfWork.GetRepository<ILogNoteRepository>().Update(logNote);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteLogNote(Guid logNoteId)
        {
            await using var unitOfWork = await User.GetConnection();

            var logNote = await GetLogNoteById(logNoteId);

            var academicYearService = new AcademicYearService(User);
            await academicYearService.IsAcademicYearLocked(logNote.AcademicYearId);

            await unitOfWork.GetRepository<ILogNoteRepository>().Delete(logNoteId);

            await unitOfWork.SaveChangesAsync();
        }
    }
}