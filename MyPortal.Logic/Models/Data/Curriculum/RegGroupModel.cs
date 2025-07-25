using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Curriculum
{
    public class RegGroupModel : EntityModel
    {
        public RegGroupModel(RegGroup model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(RegGroup model)
        {
            StudentGroupId = model.StudentGroupId;
            YearGroupId = model.YearGroupId;

            if (model.StudentGroup != null)
            {
                StudentGroup = new StudentGroupModel(model.StudentGroup);
            }

            if (model.YearGroup != null)
            {
                YearGroup = new YearGroupModel(model.YearGroup);
            }
        }

        public Guid StudentGroupId { get; set; }

        public Guid YearGroupId { get; set; }

        public virtual StudentGroupModel StudentGroup { get; set; }

        public virtual YearGroupModel YearGroup { get; set; }
    }
}