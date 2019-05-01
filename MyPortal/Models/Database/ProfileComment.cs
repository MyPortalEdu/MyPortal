namespace MyPortal.Models.Database
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Profile_Comments")]
    public partial class ProfileComment
    {
        public int Id { get; set; }

        public int CommentBankId { get; set; }

        [Required]
        public string Value { get; set; }

        public virtual ProfileCommentBank ProfileCommentBank { get; set; }
    }
}