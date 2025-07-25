using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Calendar
{
    public class DiaryEventTemplateModel : EntityModel
    {
        public Guid EventTypeId { get; set; }

        public int Minutes { get; set; }

        public int Hours { get; set; }

        public int Days { get; set; }

        public DiaryEventTypeModel DiaryEventType { get; set; }

        public DiaryEventTemplateModel(DiaryEventTemplate model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(DiaryEventTemplate model)
        {
            EventTypeId = model.EventTypeId;
            Minutes = model.Minutes;
            Hours = model.Hours;
            Days = model.Days;

            if (model.DiaryEventType != null)
            {
                DiaryEventType = new DiaryEventTypeModel(model.DiaryEventType);
            }
        }
    }
}