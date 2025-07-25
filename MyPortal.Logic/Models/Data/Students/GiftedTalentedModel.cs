using System;
using System.ComponentModel.DataAnnotations;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Data.Curriculum;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Students
{
    public class GiftedTalentedModel : EntityModel
    {
        public Guid StudentId { get; set; }
        public Guid SubjectId { get; set; }

        [Required] public string Notes { get; set; }

        public virtual StudentModel Student { get; set; }
        public virtual SubjectModel Subject { get; set; }

        public GiftedTalentedModel(GiftedTalented model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(GiftedTalented model)
        {
            StudentId = model.StudentId;
            SubjectId = model.SubjectId;
            Notes = model.Notes;

            if (model.Student != null)
            {
                Student = new StudentModel(model.Student);
            }

            if (model.Subject != null)
            {
                Subject = new SubjectModel(model.Subject);
            }
        }
    }
}