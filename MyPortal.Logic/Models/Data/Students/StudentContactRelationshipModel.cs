using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Data.Contacts;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Students
{
    public class StudentContactRelationshipModel : EntityModel
    {
        public StudentContactRelationshipModel(StudentContactRelationship model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(StudentContactRelationship model)
        {
            RelationshipTypeId = model.RelationshipTypeId;
            StudentId = model.StudentId;
            ContactId = model.ContactId;
            Correspondence = model.Correspondence;
            ParentalResponsibility = model.ParentalResponsibility;
            PupilReport = model.PupilReport;
            CourtOrder = model.CourtOrder;

            if (model.RelationshipType != null)
            {
                RelationshipType = new RelationshipTypeModel(model.RelationshipType);
            }

            if (model.Student != null)
            {
                Student = new StudentModel(model.Student);
            }

            if (model.Contact != null)
            {
                Contact = new ContactModel(model.Contact);
            }
        }

        public Guid RelationshipTypeId { get; set; }

        public Guid StudentId { get; set; }

        public Guid ContactId { get; set; }

        public bool Correspondence { get; set; }

        public bool ParentalResponsibility { get; set; }

        public bool PupilReport { get; set; }

        public bool CourtOrder { get; set; }

        public virtual RelationshipTypeModel RelationshipType { get; set; }
        public virtual StudentModel Student { get; set; }
        public virtual ContactModel Contact { get; set; }
    }
}