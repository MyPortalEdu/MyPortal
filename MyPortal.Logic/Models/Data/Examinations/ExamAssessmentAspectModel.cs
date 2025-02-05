﻿using System;
using MyPortal.Database.Interfaces;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Data.Assessment;
using MyPortal.Logic.Models.Structures;
using Task = System.Threading.Tasks.Task;

namespace MyPortal.Logic.Models.Data.Examinations
{
    public class ExamAssessmentAspectModel : BaseModelWithLoad
    {
        public ExamAssessmentAspectModel(ExamAssessmentAspect model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(ExamAssessmentAspect model)
        {
            AssessmentId = model.AssessmentId;
            AspectId = model.AspectId;
            SeriesId = model.SeriesId;
            AspectOrder = model.AspectOrder;

            if (model.Aspect != null)
            {
                Aspect = new AspectModel(model.Aspect);
            }

            if (model.Assessment != null)
            {
                Assessment = new ExamAssessmentModel(model.Assessment);
            }

            if (model.Series != null)
            {
                Series = new ExamSeriesModel(model.Series);
            }
        }

        public Guid AssessmentId { get; set; }

        public Guid AspectId { get; set; }

        public Guid SeriesId { get; set; }

        public int AspectOrder { get; set; }

        public virtual AspectModel Aspect { get; set; }
        public virtual ExamAssessmentModel Assessment { get; set; }
        public virtual ExamSeriesModel Series { get; set; }

        protected override async Task LoadFromDatabase(IUnitOfWork unitOfWork)
        {
            if (Id.HasValue)
            {
                var model = await unitOfWork.ExamAssessmentAspects.GetById(Id.Value);
                LoadFromModel(model);
            }
        }
    }
}