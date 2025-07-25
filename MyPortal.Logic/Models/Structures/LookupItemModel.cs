using System.ComponentModel.DataAnnotations;
using MyPortal.Database.Interfaces;

namespace MyPortal.Logic.Models.Structures
{
    public abstract class LookupItemModel : EntityModel
    {
        public LookupItemModel(ILookupItem model) : base(model)
        {
            Description = model.Description;
            Active = model.Active;
        }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(256)]
        public string Description { get; set; }

        public bool Active { get; set; }
    }
}