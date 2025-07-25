using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Data.Agents;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Students
{
    public class StudentAgentRelationshipModel : EntityModel
    {
        public StudentAgentRelationshipModel(StudentAgentRelationship model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(StudentAgentRelationship model)
        {
            StudentId = model.StudentId;
            AgentId = model.AgentId;
            RelationshipTypeId = model.RelationshipTypeId;

            if (model.Student != null)
            {
                Student = new StudentModel(model.Student);
            }

            if (model.Agent != null)
            {
                Agent = new AgentModel(model.Agent);
            }

            if (model.RelationshipType != null)
            {
                RelationshipType = new RelationshipTypeModel(model.RelationshipType);
            }
        }

        public Guid StudentId { get; set; }

        public Guid AgentId { get; set; }

        public Guid RelationshipTypeId { get; set; }

        public virtual StudentModel Student { get; set; }
        public virtual AgentModel Agent { get; set; }
        public virtual RelationshipTypeModel RelationshipType { get; set; }
    }
}