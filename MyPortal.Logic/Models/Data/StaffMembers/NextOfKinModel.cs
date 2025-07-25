using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Data.People;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.StaffMembers
{
    public class NextOfKinModel : EntityModel
    {
        public NextOfKinModel(NextOfKin model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(NextOfKin model)
        {
            StaffMemberId = model.StaffMemberId;
            NextOfKinPersonId = model.NextOfKinPersonId;
            RelationshipTypeId = model.RelationshipTypeId;

            if (model.StaffMember != null)
            {
                StaffMember = new StaffMemberModel(model.StaffMember);
            }

            if (model.NextOfKinPerson != null)
            {
                NextOfKinPerson = new PersonModel(model.NextOfKinPerson);
            }

            if (model.RelationshipType != null)
            {
                RelationshipType = new NextOfKinRelationshipTypeModel(model.RelationshipType);
            }
        }

        public Guid StaffMemberId { get; set; }
        public Guid NextOfKinPersonId { get; set; }
        public Guid RelationshipTypeId { get; set; }

        public virtual StaffMemberModel StaffMember { get; set; }
        public virtual PersonModel NextOfKinPerson { get; set; }
        public virtual NextOfKinRelationshipTypeModel RelationshipType { get; set; }
    }
}