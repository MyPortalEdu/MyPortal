using System;
using System.ComponentModel.DataAnnotations;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;
using MyPortal.Logic.Models.Summary;

namespace MyPortal.Logic.Models.Data.Documents
{
    public class DirectoryModel : EntityModel
    {
        public DirectoryModel(Directory model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(Directory model)
        {
            ParentId = model.ParentId;
            Name = model.Name;
            Private = model.Private;

            if (model.Parent != null)
            {
                Parent = new DirectoryModel(model.Parent);
            }
        }

        public DirectoryModel Parent { get; set; }

        public Guid? ParentId { get; set; }

        [Required] [StringLength(128)] public string Name { get; set; }

        public bool Private { get; set; }

        public DirectoryChildSummaryModel GetListModel()
        {
            return new DirectoryChildSummaryModel(this);
        }
    }
}