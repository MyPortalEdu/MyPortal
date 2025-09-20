﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyPortal.Core.Entities
{
    [Table("SubjectCodes")]
    public class SubjectCode : LookupEntity
    {
        
        [Required, StringLength(5)]
        public required string Code { get; set; }

        public Guid SubjectCodeSetId { get; set; }

        public SubjectCodeSet? SubjectCodeSet { get; set; }
    }
}