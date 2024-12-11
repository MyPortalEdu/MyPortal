using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Database.BaseTypes;

namespace MyPortal.Database.Models.Entity
{
    [Table("phone_number_type")]
    public class PhoneNumberType : LookupItem
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage",
            "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PhoneNumberType()
        {
            PhoneNumbers = new HashSet<PhoneNumber>();
        }


        public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; }
    }
}