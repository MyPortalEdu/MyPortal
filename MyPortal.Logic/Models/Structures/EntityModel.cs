using System;
using MyPortal.Database.Interfaces;

namespace MyPortal.Logic.Models.Structures
{
    public abstract class EntityModel
    {
        protected EntityModel(IEntity model)
        {
            Id = model.Id;
        }

        protected EntityModel()
        {
        }

        public Guid? Id { get; set; }
    }
}