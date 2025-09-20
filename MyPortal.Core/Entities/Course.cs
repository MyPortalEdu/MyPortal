﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("Courses")]
    public class Course : LookupEntity
    {
        public Guid SubjectId { get; set; }

        [Required, StringLength(128)]
        public required string Name { get; set; }

        public Subject? Subject { get; set; }
    }
}