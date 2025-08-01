using System;
using System.ComponentModel.DataAnnotations;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Curriculum
{
    public class StudyTopicModel : LookupItemModel
    {
        public StudyTopicModel(StudyTopic model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(StudyTopic model)
        {
            CourseId = model.CourseId;
            Name = model.Name;

            if (model.Course != null)
            {
                Course = new CourseModel(model.Course);
            }
        }

        public Guid CourseId { get; set; }

        [Required] [StringLength(128)] public string Name { get; set; }

        public virtual CourseModel Course { get; set; }
    }
}