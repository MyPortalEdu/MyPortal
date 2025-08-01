using System;
using System.ComponentModel.DataAnnotations;
using MyPortal.Database.Models.Entity;
using MyPortal.Logic.Models.Data.Curriculum;
using MyPortal.Logic.Models.Structures;

namespace MyPortal.Logic.Models.Data.Profiles;

public class CommentBankAreaModel : EntityModel
{
    public CommentBankAreaModel(CommentBankArea model) : base(model)
    {
        LoadFromModel(model);
    }

    public void LoadFromModel(CommentBankArea model)
    {
        CommentBankId = model.CommentBankId;
        CourseId = model.CourseId;
        Name = model.Name;

        if (model.CommentBank != null)
        {
            CommentBank = new CommentBankModel(model.CommentBank);
        }

        if (model.Course != null)
        {
            Course = new CourseModel(model.Course);
        }
    }

    public Guid CommentBankId { get; set; }

    public Guid CourseId { get; set; }

    [Required] [StringLength(256)] public string Name { get; set; }

    public virtual CommentBankModel CommentBank { get; set; }
    public virtual CourseModel Course { get; set; }
}