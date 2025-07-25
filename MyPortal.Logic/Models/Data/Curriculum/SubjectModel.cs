using System;
using System.ComponentModel.DataAnnotations;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Curriculum
{
    public class SubjectModel : EntityModel
    {
        public SubjectModel(Subject model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(Subject model)
        {
            SubjectCodeId = model.SubjectCodeId;
            Name = model.Name;
            Code = model.Code;
            Deleted = model.Deleted;

            if (model.SubjectCode != null)
            {
                SubjectCode = new SubjectCodeModel(model.SubjectCode);
            }
        }

        public Guid SubjectCodeId { get; set; }

        [Required] [StringLength(256)] public string Name { get; set; }

        [Required] [StringLength(5)] public string Code { get; set; }

        public bool Deleted { get; set; }

        public virtual SubjectCodeModel SubjectCode { get; set; }
    }
}