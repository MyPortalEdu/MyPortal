using System;
using System.ComponentModel.DataAnnotations;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Profiles;

public class CommentBankSectionModel : EntityModel
{
    public CommentBankSectionModel(CommentBankSection model) : base(model)
    {
        LoadFromModel(model);
    }

    public void LoadFromModel(CommentBankSection model)
    {
        CommentBankAreaId = model.CommentBankAreaId;
        Name = model.Name;

        if (model.Area != null)
        {
            Area = new CommentBankAreaModel(model.Area);
        }
    }

    public Guid CommentBankAreaId { get; set; }

    [Required] [StringLength(256)] public string Name { get; set; }

    public virtual CommentBankAreaModel Area { get; set; }
}