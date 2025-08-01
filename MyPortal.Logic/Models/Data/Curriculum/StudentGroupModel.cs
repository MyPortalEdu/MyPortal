using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Curriculum
{
    public class StudentGroupModel : LookupItemModel
    {
        public StudentGroupModel(StudentGroup model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(StudentGroup model)
        {
            Code = model.Code;
            Description = model.Description;
            PromoteToGroupId = model.PromoteToGroupId;
            MainSupervisorId = model.MainSupervisorId;
            MaxMembers = model.MaxMembers;
            Notes = model.Notes;
            System = model.System;

            if (model.PromoteToGroup != null)
            {
                PromoteToGroup = new StudentGroupModel(model.PromoteToGroup);
            }

            if (model.MainSupervisor != null)
            {
                MainSupervisor = new StudentGroupSupervisorModel(model.MainSupervisor);
            }
        }

        public string Code { get; set; }

        public Guid? PromoteToGroupId { get; set; }

        public Guid? MainSupervisorId { get; set; }

        public int? MaxMembers { get; set; }

        public string Notes { get; set; }

        public bool System { get; set; }

        public StudentGroupModel PromoteToGroup { get; set; }
        public StudentGroupSupervisorModel MainSupervisor { get; set; }
    }
}