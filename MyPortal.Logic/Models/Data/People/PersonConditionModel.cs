using System;
using System.ComponentModel.DataAnnotations;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Data.Medical;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.People
{
    public class PersonConditionModel : EntityModel
    {
        public PersonConditionModel(PersonCondition model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(PersonCondition model)
        {
            PersonId = model.PersonId;
            ConditionId = model.ConditionId;
            MedicationTaken = model.MedicationTaken;
            Medication = model.Medication;

            if (model.Person != null)
            {
                Person = new PersonModel(model.Person);
            }

            if (model.MedicalCondition != null)
            {
                MedicalCondition = new MedicalConditionModel(model.MedicalCondition);
            }
        }

        public Guid PersonId { get; set; }

        public Guid ConditionId { get; set; }

        public bool MedicationTaken { get; set; }

        [StringLength(256)] public string Medication { get; set; }

        public virtual PersonModel Person { get; set; }
        public virtual MedicalConditionModel MedicalCondition { get; set; }
    }
}