using System;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Data.Students;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Finance
{
    public class BasketItemModel : EntityModel
    {
        public BasketItemModel(BasketItem model) : base(model)
        {
            LoadFromModel(model);
        }

        private void LoadFromModel(BasketItem model)
        {
            StudentId = model.StudentId;
            ProductId = model.ProductId;
            Quantity = model.Quantity;

            if (model.Student != null)
            {
                Student = new StudentModel(model.Student);
            }

            if (model.Product != null)
            {
                Product = new ProductModel(model.Product);
            }
        }

        public Guid StudentId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public virtual StudentModel Student { get; set; }

        public virtual ProductModel Product { get; set; }
    }
}