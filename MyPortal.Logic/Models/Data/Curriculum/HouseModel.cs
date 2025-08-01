using System;
using System.ComponentModel.DataAnnotations;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Constants;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Curriculum
{
    public class HouseModel : EntityModel
    {
        public HouseModel(House model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(House model)
        {
            StudentGroupId = model.StudentGroupId;
            ColourCode = model.ColourCode;

            if (model.StudentGroup != null)
            {
                StudentGroup = new StudentGroupModel(model.StudentGroup);
            }
        }

        public Guid StudentGroupId { get; set; }

        [StringLength(128)]
        [RegularExpression(RegularExpressions.ColourCode)]
        public string ColourCode { get; set; }

        public virtual StudentGroupModel StudentGroup { get; set; }
    }
}