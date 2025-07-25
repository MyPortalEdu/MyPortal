using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Examinations
{
    public class ExamBaseComponentModel : EntityModel
    {
        public ExamBaseComponentModel(ExamBaseComponent model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(ExamBaseComponent model)
        {
            AssessmentModeId = model.AssessmentModeId;
            ExamAssessmentId = model.ExamAssessmentId;
            ComponentCode = model.ComponentCode;

            if (model.AssessmentMode != null)
            {
                AssessmentMode = new ExamAssessmentModeModel(model.AssessmentMode);
            }

            if (model.Assessment != null)
            {
                Assessment = new ExamAssessmentModel(model.Assessment);
            }
        }

        public Guid AssessmentModeId { get; set; }

        public Guid ExamAssessmentId { get; set; }

        public string ComponentCode { get; set; }

        public virtual ExamAssessmentModeModel AssessmentMode { get; set; }
        public virtual ExamAssessmentModel Assessment { get; set; }
    }
}