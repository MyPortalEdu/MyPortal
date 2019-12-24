﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Data.Models
{
    [Table("PhoneNumberType", Schema = "communication")]
    public class PhoneNumberType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PhoneNumberType()
        {
            PhoneNumbers = new HashSet<PhoneNumber>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string Description { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PhoneNumber> PhoneNumbers { get; set; }
    }
}