using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Database.Models.Entity;

[Table("sen_review_status")]
public class SenReviewStatus : BaseTypes.LookupItem
{
    public virtual ICollection<SenReview> SenReviews { get; set; }
}