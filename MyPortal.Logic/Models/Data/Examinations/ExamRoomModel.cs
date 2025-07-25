using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Data.School;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Examinations
{
    public class ExamRoomModel : EntityModel
    {
        public ExamRoomModel(ExamRoom model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(ExamRoom model)
        {
            RoomId = model.RoomId;
            Columns = model.Columns;
            Rows = model.Rows;

            if (model.Room != null)
            {
                Room = new RoomModel(model.Room);
            }
        }

        public Guid RoomId { get; set; }

        public int Columns { get; set; }

        public int Rows { get; set; }

        public virtual RoomModel Room { get; set; }
    }
}