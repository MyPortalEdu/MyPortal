﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Database.Models.Entity
{
    [Table("AttendanceCodeTypes")]
    public class AttendanceCodeType : BaseTypes.Entity
    {
        public AttendanceCodeType()
        {
            Codes = new HashSet<AttendanceCode>();
        }

        [Column(Order = 1)]
        [Required]
        [StringLength(256)]
        public string Description { get; set; }

        public virtual ICollection<AttendanceCode> Codes { get; set; }
    }
}