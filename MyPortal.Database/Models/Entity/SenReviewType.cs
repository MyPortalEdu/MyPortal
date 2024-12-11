using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Database.BaseTypes;

namespace MyPortal.Database.Models.Entity
{
    [Table("sen_review_type")]
    public class SenReviewType : LookupItem
    {
        public virtual ICollection<SenReview> Reviews { get; set; }
    }
}