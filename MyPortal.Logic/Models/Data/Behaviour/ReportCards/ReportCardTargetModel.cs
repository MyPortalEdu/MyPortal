using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Behaviour.ReportCards
{
    public class ReportCardTargetModel : EntityModel
    {
        public ReportCardTargetModel(ReportCardTarget model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(ReportCardTarget model)
        {
            ReportCardId = model.ReportCardId;
            TargetId = model.TargetId;

            if (model.ReportCard != null)
            {
                ReportCard = new ReportCardModel(model.ReportCard);
            }

            if (model.Target != null)
            {
                Target = new BehaviourTargetModel(model.Target);
            }
        }

        public Guid ReportCardId { get; set; }

        public Guid TargetId { get; set; }

        public virtual ReportCardModel ReportCard { get; set; }
        public virtual BehaviourTargetModel Target { get; set; }
    }
}