using System;
using System.ComponentModel.DataAnnotations;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Data.People;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Contacts
{
    public class ContactModel : EntityModel
    {
        public ContactModel(Contact model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(Contact model)
        {
            PersonId = model.PersonId;
            ParentalBallot = model.ParentalBallot;
            PlaceOfWork = model.PlaceOfWork;
            JobTitle = model.JobTitle;
            NiNumber = model.NiNumber;

            if (model.Person != null)
            {
                Person = new PersonModel(model.Person);
            }
        }

        public Guid PersonId { get; set; }

        public bool ParentalBallot { get; set; }

        [StringLength(256)] public string PlaceOfWork { get; set; }

        [StringLength(256)] public string JobTitle { get; set; }

        [StringLength(128)] public string NiNumber { get; set; }

        public virtual PersonModel Person { get; set; }
    }
}