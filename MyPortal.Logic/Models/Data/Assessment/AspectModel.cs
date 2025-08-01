using System;
using System.ComponentModel.DataAnnotations;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Assessment
{
    public class AspectModel : LookupItemModel
    {
        public AspectModel(Aspect model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(Aspect model)
        {
            TypeId = model.TypeId;
            GradeSetId = model.GradeSetId;
            MinMark = model.MinMark;
            MaxMark = model.MaxMark;
            Name = model.Name;
            ColumnHeading = model.ColumnHeading;
            Private = model.Private;

            if (model.Type != null)
            {
                Type = new AspectTypeModel(model.Type);
            }

            if (model.GradeSet != null)
            {
                GradeSet = new GradeSetModel(model.GradeSet);
            }
        }

        public Guid TypeId { get; set; }

        public Guid? GradeSetId { get; set; }

        public decimal? MinMark { get; set; }

        public decimal? MaxMark { get; set; }

        [Required] [StringLength(128)] public string Name { get; set; }

        [Required] [StringLength(50)] public string ColumnHeading { get; set; }

        public bool Private { get; set; }

        public virtual AspectTypeModel Type { get; set; }

        public virtual GradeSetModel GradeSet { get; set; }
    }
}