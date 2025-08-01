using System;
using System.ComponentModel.DataAnnotations;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.School
{
    public class RoomClosureModel : EntityModel
    {
        public RoomClosureModel(RoomClosure model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(RoomClosure model)
        {
            RoomId = model.RoomId;
            ReasonId = model.ReasonId;
            StartDate = model.StartDate;
            EndDate = model.EndDate;
            Notes = model.Notes;

            if (model.Room != null)
            {
                Room = new RoomModel(model.Room);
            }

            if (model.Reason != null)
            {
                Reason = new RoomClosureReasonModel(model.Reason);
            }
        }


        public Guid RoomId { get; set; }

        public Guid ReasonId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [StringLength(256)] public string Notes { get; set; }

        public virtual RoomModel Room { get; set; }
        public virtual RoomClosureReasonModel Reason { get; set; }
    }
}