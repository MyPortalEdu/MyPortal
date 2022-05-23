﻿using System.ComponentModel.DataAnnotations.Schema;
using MyPortal.Database.BaseTypes;

namespace MyPortal.Database.Models.Entity;

[Table("BehaviourRoleTypes")]
public class BehaviourRoleType : LookupItem
{
    [Column(Order = 3)]
    public int DefaultPoints { get; set; }
}