﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("AttendanceCodeTypes")]
    public class AttendanceCodeType : Entity
    {
        [Required, StringLength(256)]
        public required string Description { get; set; }
    }
}