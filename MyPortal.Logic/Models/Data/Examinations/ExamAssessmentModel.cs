using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Examinations
{
    public class ExamAssessmentModel : EntityModel
    {
        public ExamAssessmentModel(ExamAssessment model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(ExamAssessment model)
        {
            ExamBoardId = model.ExamBoardId;
            AssessmentType = (int)model.AssessmentType;
            InternalTitle = model.InternalTitle;
            ExternalTitle = model.ExternalTitle;

            if (model.ExamBoard != null)
            {
                ExamBoard = new ExamBoardModel(model.ExamBoard);
            }
        }

        public Guid ExamBoardId { get; set; }

        public int AssessmentType { get; set; }

        public string InternalTitle { get; set; }

        public string ExternalTitle { get; set; }

        public virtual ExamBoardModel ExamBoard { get; set; }
    }
}