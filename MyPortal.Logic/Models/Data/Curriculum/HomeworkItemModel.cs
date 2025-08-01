using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Data.Documents;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Curriculum
{
    public class HomeworkItemModel : EntityModel
    {
        public HomeworkItemModel(HomeworkItem model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(HomeworkItem model)
        {
            DirectoryId = model.DirectoryId;
            Title = model.Title;
            Description = model.Description;
            SubmitOnline = model.SubmitOnline;
            MaxPoints = model.MaxPoints;

            if (model.Directory != null)
            {
                Directory = new DirectoryModel(model.Directory);
            }
        }

        public int MaxPoints { get; set; }

        public Guid DirectoryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool SubmitOnline { get; set; }

        public virtual DirectoryModel Directory { get; set; }
    }
}