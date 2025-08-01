using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.School
{
    public class BuildingFloorModel : LookupItemModel
    {
        public BuildingFloorModel(BuildingFloor model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(BuildingFloor model)
        {
            BuildingId = model.BuildingId;


            if (model.Building != null)
            {
                Building = new BuildingModel(model.Building);
            }
        }

        public Guid BuildingId { get; set; }

        public BuildingModel Building { get; set; }
    }
}