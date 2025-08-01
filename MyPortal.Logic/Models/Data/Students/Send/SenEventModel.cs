using System;
using System.ComponentModel.DataAnnotations;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Students.Send
{
    public class SenEventModel : EntityModel
    {
        public SenEventModel(SenEvent model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(SenEvent model)
        {
            StudentId = model.StudentId;
            EventTypeId = model.EventTypeId;
            Date = model.Date;
            Note = model.Note;

            if (model.Student != null)
            {
                Student = new StudentModel(model.Student);
            }

            if (model.Type != null)
            {
                Type = new SenEventTypeModel(model.Type);
            }
        }

        public Guid StudentId { get; set; }

        public Guid EventTypeId { get; set; }

        public DateTime Date { get; set; }

        [Required] public string Note { get; set; }

        public virtual StudentModel Student { get; set; }

        public virtual SenEventTypeModel Type { get; set; }
    }
}