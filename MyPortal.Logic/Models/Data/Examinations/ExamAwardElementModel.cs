using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Examinations
{
    public class ExamAwardElementModel : EntityModel
    {
        public ExamAwardElementModel(ExamAwardElement model) : base(model)
        {
        }

        private void LoadFromModel(ExamAwardElement model)
        {
            AwardId = model.AwardId;
            ElementId = model.ElementId;

            if (model.Award != null)
            {
                Award = new ExamAwardModel(model.Award);
            }

            if (model.Element != null)
            {
                Element = new ExamElementModel(model.Element);
            }
        }

        public Guid AwardId { get; set; }

        public Guid ElementId { get; set; }

        public virtual ExamAwardModel Award { get; set; }
        public virtual ExamElementModel Element { get; set; }
    }
}