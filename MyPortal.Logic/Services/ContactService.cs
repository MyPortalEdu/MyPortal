﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Helpers;
using MyPortal.Logic.Interfaces;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.Students;
using MyPortal.Logic.Models.Requests.Contact;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Logic.Services
{
    public sealed class ContactService : BaseService, IContactService
    {
        public ContactService(ISessionUser user) : base(user)
        {
        }

        public async Task<IEnumerable<StudentModel>> GetReportableStudents(Guid contactId)
        {
            await using var unitOfWork = await User.GetConnection();

            var students = await unitOfWork.Students.GetByContact(contactId, true);

            return students.Select(s => new StudentModel(s));
        }

        public async Task CreateContact(ContactRequestModel model)
        {
            Validate(model);

            await using var unitOfWork = await User.GetConnection();

            var contact = new Contact
            {
                Id = Guid.NewGuid(),
                JobTitle = model.JobTitle,
                NiNumber = model.NiNumber,
                PlaceOfWork = model.PlaceOfWork,
                ParentalBallot = model.ParentalBallot,
                Person = PersonHelper.CreatePersonFromModel(model)
            };

            unitOfWork.Contacts.Create(contact);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateContact(Guid contactId, ContactRequestModel model)
        {
            Validate(model);

            await using var unitOfWork = await User.GetConnection();

            var contact = await unitOfWork.Contacts.GetById(contactId);

            contact.JobTitle = model.JobTitle;
            contact.NiNumber = model.NiNumber;
            contact.PlaceOfWork = model.PlaceOfWork;
            contact.ParentalBallot = model.ParentalBallot;

            PersonHelper.UpdatePersonFromModel(contact.Person, model);

            await unitOfWork.People.Update(contact.Person);
            await unitOfWork.Contacts.Update(contact);

            await unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteContact(Guid contactId)
        {
            await using var unitOfWork = await User.GetConnection();

            await unitOfWork.Contacts.Delete(contactId);
        }
    }
}