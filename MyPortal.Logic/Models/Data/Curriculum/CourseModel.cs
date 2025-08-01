using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Curriculum
{
    public class CourseModel : LookupItemModel
    {
        public CourseModel(Course model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(Course model)
        {
            SubjectId = model.SubjectId;
            Name = model.Name;

            if (model.Subject != null)
            {
                Subject = new SubjectModel(model.Subject);
            }
        }

        public Guid SubjectId { get; set; }

        public string Name { get; set; }

        public virtual SubjectModel Subject { get; set; }
    }
}