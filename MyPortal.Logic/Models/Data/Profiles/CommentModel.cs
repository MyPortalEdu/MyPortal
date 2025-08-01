using System;
using System.ComponentModel.DataAnnotations;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Profiles
{
    public class CommentModel : EntityModel
    {
        public CommentModel(Comment model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(Comment model)
        {
            CommentBankSectionId = model.CommentBankSectionId;
            Value = model.Value;

            if (model.Section != null)
            {
                Section = new CommentBankSectionModel(model.Section);
            }
        }

        public Guid CommentBankSectionId { get; set; }

        [Required] public string Value { get; set; }

        public CommentBankSectionModel Section { get; set; }
    }
}