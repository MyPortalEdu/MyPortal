using System;
using System.Threading.Tasks;
using MyPortal.Database.Interfaces.Repositories;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Exceptions;
using MyPortal.Logic.Helpers;
using MyPortal.Logic.Interfaces;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.StaffMembers;
using MyPortal.Logic.Models.Requests.StaffMember;

namespace MyPortal.Logic.Services
{
    public sealed class StaffMemberService : BaseService, IStaffMemberService
    {
        public StaffMemberService(ISessionUser user) : base(user)
        {
        }

        public async Task<bool> IsLineManager(Guid staffMemberId, Guid lineManagerId)
        {
            await using var unitOfWork = await User.GetConnection();

            var staffMember = await unitOfWork.GetRepository<IStaffMemberRepository>().GetById(staffMemberId);

            if (staffMember == null)
            {
                throw new NotFoundException("Staff member not found.");
            }

            if (staffMember.LineManagerId == null)
            {
                return false;
            }

            if (staffMember.LineManagerId == lineManagerId)
            {
                return true;
            }

            return await IsLineManager(staffMember.LineManagerId.Value, lineManagerId);
        }

        public async Task<StaffMemberModel> GetById(Guid staffMemberId)
        {
            await using var unitOfWork = await User.GetConnection();

            var staffMember = await unitOfWork.GetRepository<IStaffMemberRepository>().GetById(staffMemberId);

            if (staffMember == null)
            {
                throw new NotFoundException("Staff member not found.");
            }

            return new StaffMemberModel(staffMember);
        }

        public async Task<StaffMemberModel> GetByPersonId(Guid personId, bool throwIfNotFound = true)
        {
            await using var unitOfWork = await User.GetConnection();

            var staffMember = await unitOfWork.GetRepository<IStaffMemberRepository>().GetByPersonId(personId);

            if (staffMember == null && throwIfNotFound)
            {
                throw new NotFoundException("Staff member not found.");
            }

            return new StaffMemberModel(staffMember);
        }

        public async Task<StaffMemberModel> GetByUserId(Guid userId, bool throwIfNotFound = true)
        {
            await using var unitOfWork = await User.GetConnection();

            var staffMember = await unitOfWork.GetRepository<IStaffMemberRepository>().GetByUserId(userId);

            if (staffMember == null && throwIfNotFound)
            {
                throw new NotFoundException("Staff member not found.");
            }

            return new StaffMemberModel(staffMember);
        }

        public async System.Threading.Tasks.Task CreateStaffMember(StaffMemberRequestModel model)
        {
            Validate(model);

            await using var unitOfWork = await User.GetConnection();

            var staffMember = new StaffMember
            {
                Id = Guid.NewGuid(),
                LineManagerId = model.LineManagerId,
                Code = model.Code,
                BankName = model.BankName,
                BankAccount = model.BankAccount,
                BankSortCode = model.BankSortCode,
                NiNumber = model.NiNumber,
                Qualifications = model.Qualifications,
                TeachingStaff = model.TeachingStaff,
                Person = PersonHelper.CreatePersonFromModel(model)
            };

            unitOfWork.GetRepository<IStaffMemberRepository>().Create(staffMember);

            await unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UpdateStaffMember(Guid staffMemberId, StaffMemberRequestModel model)
        {
            Validate(model);

            await using var unitOfWork = await User.GetConnection();

            var staffMemberInDb = await unitOfWork.GetRepository<IStaffMemberRepository>().GetById(staffMemberId);

            if (staffMemberInDb == null)
            {
                throw new NotFoundException("Staff member not found.");
            }

            staffMemberInDb.LineManagerId = model.LineManagerId;
            staffMemberInDb.PersonId = model.PersonId;
            staffMemberInDb.Code = model.Code;
            staffMemberInDb.BankName = model.BankName;
            staffMemberInDb.BankAccount = model.BankAccount;
            staffMemberInDb.BankSortCode = model.BankSortCode;
            staffMemberInDb.NiNumber = model.NiNumber;
            staffMemberInDb.Qualifications = model.Qualifications;
            staffMemberInDb.TeachingStaff = model.TeachingStaff;

            PersonHelper.UpdatePersonFromModel(staffMemberInDb.Person, model);

            await unitOfWork.GetRepository<IPersonRepository>().Update(staffMemberInDb.Person);
            await unitOfWork.GetRepository<IStaffMemberRepository>().Update(staffMemberInDb);

            await unitOfWork.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task DeleteStaffMember(Guid staffMemberId)
        {
            await using var unitOfWork = await User.GetConnection();

            var staffMemberInDb = await unitOfWork.GetRepository<IStaffMemberRepository>().GetById(staffMemberId);

            if (staffMemberInDb == null)
            {
                throw new NotFoundException("Staff member not found.");
            }

            await unitOfWork.GetRepository<IStaffMemberRepository>().Delete(staffMemberId);

            await unitOfWork.SaveChangesAsync();
        }
    }
}