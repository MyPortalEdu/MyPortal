﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Database.BaseTypes;
using MyPortal.Database.Interfaces;

namespace MyPortal.Database.Models.Entity
{
    // FIXED
    [Table("ExclusionReasons")]
    public class ExclusionReason : LookupItem, ISystemEntity, ICensusEntity
    {
        public ExclusionReason()
        {
            Exclusions = new HashSet<Exclusion>();
        }

        [Column(Order = 4)] public string Code { get; set; }

        [Column(Order = 5)] public bool System { get; set; }

        public virtual ICollection<Exclusion> Exclusions { get; set; }
    }
}