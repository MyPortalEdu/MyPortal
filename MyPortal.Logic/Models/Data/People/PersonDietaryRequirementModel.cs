using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Data.Medical;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.People
{
    public class PersonDietaryRequirementModel : EntityModel
    {
        public PersonDietaryRequirementModel(PersonDietaryRequirement model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(PersonDietaryRequirement model)
        {
            PersonId = model.PersonId;
            DietaryRequirementId = model.DietaryRequirementId;

            if (model.Person != null)
            {
                Person = new PersonModel(model.Person);
            }

            if (model.DietaryRequirement != null)
            {
                DietaryRequirement = new DietaryRequirementModel(model.DietaryRequirement);
            }
        }

        public Guid PersonId { get; set; }

        public Guid DietaryRequirementId { get; set; }

        public virtual DietaryRequirementModel DietaryRequirement { get; set; }
        public virtual PersonModel Person { get; set; }
    }
}